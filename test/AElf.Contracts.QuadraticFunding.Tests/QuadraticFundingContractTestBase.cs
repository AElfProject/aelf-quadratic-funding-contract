using AElf.Boilerplate.TestBase;
using AElf.Contracts.MultiToken;
using AElf.Cryptography.ECDSA;
using AElf.Kernel.Token;
using AElf.Types;

namespace AElf.Contracts.QuadraticFunding
{
    public class QuadraticFundingContractTestBase : DAppContractTestBase<QuadraticFundingContractTestModule>
    {
        // You can get address of any contract via GetAddress method, for example:
        internal Address TokenContractAddress => GetAddress(TokenSmartContractAddressNameProvider.StringName);

        internal QuadraticFundingContractContainer.QuadraticFundingContractStub GetQuadraticFundingContractStub(
            ECKeyPair senderKeyPair)
        {
            return GetTester<QuadraticFundingContractContainer.QuadraticFundingContractStub>(DAppContractAddress,
                senderKeyPair);
        }

        internal TokenContractContainer.TokenContractStub GetTokenContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<TokenContractContainer.TokenContractStub>(TokenContractAddress, senderKeyPair);
        }
    }
}