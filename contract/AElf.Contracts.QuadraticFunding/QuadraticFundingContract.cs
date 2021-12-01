using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.QuadraticFunding
{
    public partial class QuadraticFundingContract : QuadraticFundingContractContainer.QuadraticFundingContractBase
    {
        public override Empty Initialize(InitializeInput input)
        {
            Assert(State.Owner.Value == null, "Already initialized.");
            State.TaxPoint.Value = 100;
            State.VoteSymbol.Value = string.IsNullOrEmpty(input.VoteSymbol)
                ? Context.Variables.NativeSymbol
                : input.VoteSymbol;
            State.CurrentRound.Value = 1;
            State.Interval.Value = input.Interval == 0 ? DefaultInterval : input.Interval; // 60 days.
            State.Owner.Value = input.Owner ?? Context.Sender;
            State.FeeSetter.Value = input.Owner ?? Context.Sender;
            State.BasicVotingUnit.Value = input.BasicVotingUnit == 0 ? DefaultBasicVotingUnit : input.BasicVotingUnit;

            State.TokenContract.Value =
                Context.GetContractAddressByName(SmartContractConstants.TokenContractSystemName);
            return new Empty();
        }
    }
}