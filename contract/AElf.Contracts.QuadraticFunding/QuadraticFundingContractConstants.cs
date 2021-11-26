namespace AElf.Contracts.QuadraticFunding
{
    public partial class QuadraticFundingContract
    {
        private const long MaxTaxPoint = 5000;
        // The decimals of ELF is 8.
        private const long DefaultBasicVotingUnit = 1_00000000;
        private const long DefaultInterval = 60 * 24 * 3600;
    }
}