using Chia_Client_API.FullNodeAPI_NS;
using Chia_Client_API.WalletAPI_NS;
using CHIA_RPC.FullNode_NS;
using CHIA_RPC.General_NS;
using CHIA_RPC.Wallet_NS.Wallet_NS;
using Microsoft.AspNetCore.Mvc;
using NFT.Storage.Net.ClientResponse;

namespace SkymeyBlockChain.Controllers
{
    [ApiController]
    [Route("api/Blockchain")]
    public class BlockchainController : ControllerBase
    {

        private readonly ILogger<BlockchainController> _logger;
        private static Wallet_RPC_Client Wallet = new Wallet_RPC_Client(reportResponseErrors: false);
        private static FullNode_RPC_Client node = new FullNode_RPC_Client(reportResponseErrors: false);

        public BlockchainController(ILogger<BlockchainController> logger)
        {
            _logger = logger;
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
            var space = node.GetNetworkSpace_Sync(new GetNetworkSpace_RPC("0xf432a8f256f9da0cb695f29592140aeb98706c9b8a73881c52d152e9ba3cc769", "0x6141b39fa722597da4d9130df1c4d30a98f671da26e01c363a1ac286112b0dfa"));
            return space.space_eb;
        }
    }
}
