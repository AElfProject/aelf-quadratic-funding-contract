using System;
using System.Linq;
using AElf.CSharp.Core;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.QuadraticFunding
{
    public partial class QuadraticFundingContract
    {
        public override ProjectList GetAllProjects(Int64Value input)
        {
            return State.ProjectListMap[input.Value];
        }

        public override RankingList GetRankingList(Int64Value input)
        {
            var round = input.Value;
            var rankingList = new RankingList();
            if (State.TotalSupportAreaMap[round] != 0)
            {
                rankingList.Unit = State.SupportPoolMap[round].Div(State.TotalSupportAreaMap[round]);
            }

            rankingList.Projects.AddRange(State.ProjectListMap[round].Value);
            foreach (var projectId in rankingList.Projects)
            {
                var project = State.ProjectMap[projectId];
                if (project == null) continue;
                rankingList.Votes.Add(project.TotalVotes);
                var support = State.BanMap[projectId] ? 0 : project.SupportArea;
                rankingList.Support.Add(support);
                rankingList.Grants.Add(project.Grants);
            }

            return rankingList;
        }

        public override RankingList GetPagedRankingList(GetPagedRankingListInput input)
        {
            var round = input.Round;
            var rankingList = new RankingList();
            var totalSupportArea = State.TotalSupportAreaMap[round];
            if (totalSupportArea != 0)
            {
                rankingList.Unit = State.SupportPoolMap[round].Div(totalSupportArea);
            }

            var fullProjects = State.ProjectListMap[round];

            var start = input.Page.Mul(input.Size);
            var end = start.Add(input.Size);
            for (var i = start; i < end; i++)
            {
                if (i >= fullProjects.Value.Count)
                {
                    break;
                }

                var projectId = fullProjects.Value[(int) i];
                var project = State.ProjectMap[projectId];
                rankingList.Projects.Add(projectId);
                rankingList.Votes.Add(project.TotalVotes);
                var support = State.BanMap[projectId] ? 0 : project.SupportArea;
                rankingList.Support.Add(support);
                rankingList.Grants.Add(project.Grants);
            }

            return rankingList;
        }

        public override RoundInfo GetRoundInfo(Int64Value input)
        {
            var round = input.Value;
            var roundInfo = new RoundInfo
            {
                StartFrom = State.StartTimeMap[round],
                EndAt = State.EndTimeMap[round],
                Support = State.SupportPoolMap[round],
                PreTaxSupport = State.PreTaxSupportPoolMap[round]
            };
            return roundInfo;
        }

        public override VotingCost GetVotingCost(GetVotingCostInput input)
        {
            var votingCost = new VotingCost();
            var project = State.ProjectMap[input.ProjectId];
            var currentRound = State.CurrentRound.Value;
            votingCost.Votable = project.Round == currentRound &&
                                 Context.CurrentBlockTime < State.EndTimeMap[currentRound];

            var voted = State.VotedMap[input.ProjectId][input.From];
            var votingPoints = input.Votes.Mul(input.Votes.Add(1)).Div(2);
            votingPoints = votingPoints.Add(input.Votes.Mul(voted));
            votingCost.Cost = votingPoints.Mul(State.VotingUnitMap[project.Round]);
            return votingCost;
        }

        public override Grants GetGrantsOf(StringValue input)
        {
            var grants = new Grants();
            var project = State.ProjectMap[input.Value];
            var round = project.Round;
            if (round == 0)
            {
                return grants;
            }

            grants.Total = project.Grants;
            if (round < State.CurrentRound.Value)
            {
                // Round ends.
                if (State.TotalSupportAreaMap[round] != 0 && !State.BanMap[input.Value])
                {
                    grants.Total = grants.Total.Add(project.SupportArea.Mul(State.SupportPoolMap[round])
                        .Div(State.TotalSupportAreaMap[round]));
                }
            }

            grants.Rest = grants.Total.Sub(project.Withdrew);
            grants.Rest = Math.Max(grants.Rest, 0);
            return grants;
        }

        public override Project GetProjectOf(StringValue input)
        {
            var project = State.ProjectMap[input.Value];
            if (project == null)
            {
                return new Project();
            }

            if (State.BanMap[input.Value])
            {
                project.SupportArea = 0;
            }

            return project;
        }

        public override StringValue CalculateProjectId(CalculateProjectIdInput input)
        {
            var address = input.Address ?? Context.Sender;
            var featureValue = CalculateSenderFeatureValue(address);
            return new StringValue
            {
                Value = $"{input.Bid}{featureValue.PadLeft(10, '0')}"
            };
        }

        public override Int64Value GetCurrentRound(Empty input)
        {
            return new Int64Value {Value = State.CurrentRound.Value};
        }

        public override Int64Value GetTaxPoint(Empty input)
        {
            return new Int64Value {Value = State.TaxPoint.Value};
        }

        public override Int64Value GetTax(Empty input)
        {
            return new Int64Value {Value = State.Tax.Value};
        }

        public override Int64Value GetInterval(Empty input)
        {
            return new Int64Value {Value = State.Interval.Value};
        }

        public override Int64Value GetVotingUnit(Empty input)
        {
            return new Int64Value {Value = State.BasicVotingUnit.Value};
        }

        public override BoolValue IsProjectBanned(StringValue input)
        {
            return new BoolValue
            {
                Value = State.BanMap[input.Value]
            };
        }

        /// <summary>
        /// Upper limit 2147483647 * 2.
        /// Must be unique.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private string CalculateSenderFeatureValue(Address address)
        {
            var hash = HashHelper.ComputeFrom(address);
            var originInteger = hash.ToByteArray().ToInt32(true);
            var addMaxValue = (long) originInteger + int.MaxValue;
            return addMaxValue.ToString();
        }

        private string CalculateSenderFeatureValue(string projectId)
        {
            var length = projectId.Length;
            return long.Parse(projectId.Substring(length - 10)).ToString();
        }
    }
}