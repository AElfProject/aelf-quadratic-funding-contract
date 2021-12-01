using AElf.Standards.ACS1;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.QuadraticFunding
{
    public partial class QuadraticFundingContract
    {
        public override MethodFees GetMethodFee(StringValue input)
        {
            return new MethodFees
            {
                MethodName = input.Value,
                IsSizeFeeFree = true
            };
        }

        public override Empty SetMethodFee(MethodFees input)
        {
            // TODO
            return new Empty();
        }

        public override Empty ChangeMethodFeeController(AuthorityInfo input)
        {
            Assert(Context.Sender == State.FeeSetter.Value, "No permission.");
            State.FeeSetter.Value = input.OwnerAddress;
            return new Empty();
        }

        public override AuthorityInfo GetMethodFeeController(Empty input)
        {
            return new AuthorityInfo
            {
                OwnerAddress = State.FeeSetter.Value
            };
        }
    }
}