using Chia_Client_API.FullNodeAPI_NS;
using Chia_Client_API.WalletAPI_NS;
using CHIA_RPC.FullNode_NS;
using CHIA_RPC.General_NS;
using CHIA_RPC.Wallet_NS.Wallet_NS;
using Chilkat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Nancy;
using Nancy.Json;
using NFT.Storage.Net.ClientResponse;
using RestSharp;
using Skymey_main_lib.Models.Blockchain.Chia.Mempool;
using Skymey_main_lib.Models.JWT;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using static Skymey_main_lib.Models.Blockchain.Chia.Mempool.ChiaMempoolItems;

namespace SkymeyBlockChain.Controllers
{
    [ApiController]
    [Route("api/Blockchain")]
    public class BlockchainController : ControllerBase
    {

        private readonly ILogger<BlockchainController> _logger;
        private static Wallet_RPC_Client Wallet = new Wallet_RPC_Client(reportResponseErrors: false);
        private static FullNode_RPC_Client _node = new FullNode_RPC_Client(reportResponseErrors: false);

        public BlockchainController(ILogger<BlockchainController> logger)
        {
            _logger = logger;
            Chilkat.Cert cert = new Chilkat.Cert(); 
            bool success = cert.LoadFromFile(@"C:\Users\New\.chia\mainnet\config\ssl\full_node\private_full_node.crt");
            if (success != true)
            {
                Debug.WriteLine(cert.LastErrorText);
            }
            Chilkat.CertChain certChain = cert.GetCertChain();
            if (cert.LastMethodSuccess != true)
            {
                Debug.WriteLine(cert.LastErrorText);
            }
            Chilkat.PrivateKey privKey = new Chilkat.PrivateKey();
            success = privKey.LoadPemFile(@"C:\Users\New\.chia\mainnet\config\ssl\full_node\private_full_node.key");
            if (success != true)
            {
                Debug.WriteLine(privKey.LastErrorText);
            }
            Chilkat.Pfx pfx = new Chilkat.Pfx();
            success = pfx.AddPrivateKey(privKey, certChain);
            if (success != true)
            {
                Debug.WriteLine(pfx.LastErrorText);
            }

            // Finally, write the PFX w/ a password.
            success = pfx.ToFile("pas", @"C:\Users\New\fullnode.pfx");
            if (success != true)
            {
                Debug.WriteLine(pfx.LastErrorText);
            }
        }
        [HttpGet]
        [Route("GetBalance")]
        public async Task<List<string>> GetBalance()
        {
            List<string> balance = new List<string>();
            foreach (var item in Wallet.GetWallets_Sync().wallets)
            {
                WalletID_RPC walletID_RPC = new WalletID_RPC(item.id);
                GetWalletBalance_Response response = Wallet.GetWalletBalance_Sync(walletID_RPC);
                Console.WriteLine(response.wallet_balance.confirmed_wallet_balance_in_xch);
                balance.Add(response.wallet_balance.confirmed_wallet_balance_in_xch.ToString());
            }
            return balance;
        }

        [HttpGet]
        [Route("GetNetworkSpace")]
        public decimal GetNetworkSpace()
        {
            var space = _node.GetNetworkSpace_Sync(new GetNetworkSpace_RPC("0xf432a8f256f9da0cb695f29592140aeb98706c9b8a73881c52d152e9ba3cc769", "0x6141b39fa722597da4d9130df1c4d30a98f671da26e01c363a1ac286112b0dfa"));
            return space.space_eb;
        }

        [HttpGet]
        [Route("MempoolItems")]
        public async Task<ChiaMempoolItemsItems> MempoolItems()
        {
            var clientCertificate = new X509Certificate2(@"C:\Users\New\fullnode.pfx", "pas");
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(clientCertificate);

            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };
            var client = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:8555/get_all_mempool_items");
            var content = new StringContent("{}", null, "application/json");
            request.Content = content;
            var resp = await client.SendAsync(request);
            resp.EnsureSuccessStatusCode();
            Console.WriteLine(await resp.Content.ReadAsStringAsync());
            var jsonString = resp.Content.ReadAsStringAsync();
            var resps = JsonSerializer.Deserialize<ChiaMempoolItemsItems>(jsonString.Result);
            return resps;
        }
    }
}
