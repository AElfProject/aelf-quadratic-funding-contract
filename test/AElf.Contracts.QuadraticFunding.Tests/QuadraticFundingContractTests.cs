using System.Linq;
using System.Threading.Tasks;
using AElf.Contracts.MultiToken;
using AElf.ContractTestBase.ContractTestKit;
using AElf.CSharp.Core.Extension;
using AElf.Kernel;
using AElf.Types;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;

namespace AElf.Contracts.QuadraticFunding
{
    public class QuadraticFundingContractTests : QuadraticFundingContractTestBase
    {
        [Fact]
        public async Task InitializedTest()
        {
            // Use default values to initialize this contract.
            var stub = await Initialize();

            await stub.RoundStart.SendAsync(new Empty());

            // Check first round info.
            var roundInfo = await stub.GetRoundInfo.CallAsync(new Int64Value
            {
                Value = 1
            });
            roundInfo.StartFrom.ShouldNotBeNull();
            roundInfo.EndAt.ShouldBe(roundInfo.StartFrom.AddDays(60));
        }

        [Fact]
        public async Task DonateTest()
        {
            const long donateAmount = 10000_00000000;
            var stub = await Initialize();

            await stub.RoundStart.SendAsync(new Empty());
            await stub.Donate.SendAsync(new Int64Value
            {
                Value = donateAmount
            });

            var roundInfo = await stub.GetRoundInfo.CallAsync(new Int64Value
            {
                Value = 1
            });
            roundInfo.PreTaxSupport.ShouldBe(donateAmount);
            roundInfo.Support.ShouldBe(donateAmount - donateAmount / 100);
        }

        [Fact]
        public async Task<long> UploadProjectTest()
        {
            var stub = await Initialize();
            await stub.RoundStart.SendAsync(new Empty());
            await stub.UploadProject.SendAsync(new Empty());
            var projectId = (await stub.CalculateProjectId.CallAsync(new Address())).Value;

            var allProjects = await stub.GetAllProjects.CallAsync(new Int64Value
            {
                Value = 1
            });
            allProjects.Value.ShouldContain(projectId);

            return projectId;
        }

        [Fact]
        public async Task VoteTest()
        {
            var projectId = await UploadProjectTest();

            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var stub = GetQuadraticFundingContractStub(keyPair);

            var tokenStub = GetTokenContractStub(keyPair);

            // Need to approve first.
            await tokenStub.Approve.SendAsync(new ApproveInput
            {
                Spender = DAppContractAddress,
                Symbol = "ELF",
                Amount = long.MaxValue
            });

            const long votingUnit = 1_00000000;
            await VoteAsync(stub, projectId, 1, votingUnit * 1, CalculateGrants(1_00000000), 0);
            await VoteAsync(stub, projectId, 1, votingUnit * 2,  CalculateGrants(3_00000000), 0);
            await VoteAsync(stub, projectId, 1, votingUnit * 3,  CalculateGrants(6_00000000), 0);

            var anotherKeyPair = SampleAccount.Accounts.Skip(1).First().KeyPair;
            var anotherStub = GetQuadraticFundingContractStub(anotherKeyPair);
            var anotherTokenStub = GetTokenContractStub(anotherKeyPair);
            await tokenStub.Transfer.SendAsync(new TransferInput
            {
                To = SampleAccount.Accounts.Skip(1).First().Address,
                Symbol = "ELF",
                Amount = 100_0000_00000000
            });
            await anotherTokenStub.Approve.SendAsync(new ApproveInput
            {
                Spender = DAppContractAddress,
                Symbol = "ELF",
                Amount = long.MaxValue
            });

            await VoteAsync(anotherStub, projectId, 1, votingUnit * 4, CalculateGrants(7_00000000), 3);
            await VoteAsync(anotherStub, projectId, 1, votingUnit * 4, CalculateGrants(9_00000000), 6);
        }

        private async Task VoteAsync(QuadraticFundingContractContainer.QuadraticFundingContractStub stub,
            long projectId, long votes, long expectedCost, long expectedGrants, long expectedSupportArea)
        {
            var votingCost = (await stub.GetVotingCost.CallAsync(new GetVotingCostInput
            {
                From = SampleAccount.Accounts.First().Address,
                ProjectId = projectId,
                Votes = votes
            }));
            votingCost.Cost.ShouldBe(expectedCost);
            votingCost.Votable.ShouldBeTrue();

            await stub.Vote.SendAsync(new VoteInput
            {
                ProjectId = projectId,
                Votes = votes
            });

            // Check project.
            var project = await stub.GetProjectOf.CallAsync(new Int64Value
            {
                Value = projectId
            });
            project.Grants.ShouldBe(expectedGrants);
            project.SupportArea.ShouldBe(expectedSupportArea);
        }

        private long CalculateGrants(long preTax)
        {
            return preTax - preTax / 100;
        }

        private async Task<QuadraticFundingContractContainer.QuadraticFundingContractStub> Initialize()
        {
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var stub = GetQuadraticFundingContractStub(keyPair);
            await stub.Initialize.SendAsync(new InitializeInput());
            return stub;
        }
    }
}