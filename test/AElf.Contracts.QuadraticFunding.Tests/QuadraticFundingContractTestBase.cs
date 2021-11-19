using AElf.Boilerplate.TestBase;
using AElf.Cryptography.ECDSA;

namespace AElf.Contracts.QuadraticFunding
{
    public class QuadraticFundingContractTestBase : DAppContractTestBase<QuadraticFundingContractTestModule>
    {
        // You can get address of any contract via GetAddress method, for example:
        // internal Address DAppContractAddress => GetAddress(DAppSmartContractAddressNameProvider.StringName);

        internal QuadraticFundingContractContainer.QuadraticFundingContractStub GetQuadraticFundingContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<QuadraticFundingContractContainer.QuadraticFundingContractStub>(DAppContractAddress, senderKeyPair);
        }
    }
}