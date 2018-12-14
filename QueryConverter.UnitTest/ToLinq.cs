using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Carfup.XTBPlugins.QueryConverter.AppCode;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Tooling.Connector;

namespace Carfup.XTBPlugins.QueryConverter.UnitTest
{
    [TestClass]
    public class ToLinq
    {

        private CrmServiceClient crmSvc;
        private IOrganizationService service;
        private ConverterHelper converterHelper;

        [TestInitialize]
        public void TestInitialize()
        {
            crmSvc = new CrmServiceClient(PrivateFile.connectionString);
            service = (IOrganizationService)crmSvc.OrganizationWebProxyClient != null ? (IOrganizationService)crmSvc.OrganizationWebProxyClient : (IOrganizationService)crmSvc.OrganizationServiceProxy;
            converterHelper = new ConverterHelper(service);
            converterHelper.queryExpressionTo.LoadEntityMetadata("opportunity");
        }

        [TestMethod]
        public void ShouldGetAllColumns()
        {
            var columnSet = new ColumnSet(true);

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageColumsetToLinq(columnSet), "");
        }

        [TestMethod]
        public void ShouldGetMultipleColumns()
        {
            var columnSet = new ColumnSet("name", "opportunityid");

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageColumsetToLinq(columnSet),
                $"{Environment.NewLine}.Select(col => new {{col.Name, col.OpportunityId}})");
        }

        [TestMethod]
        public void ShouldGetSingleOrderByAsc()
        {
            var orderByAsc = new List<OrderExpression>() {
                new OrderExpression("name", OrderType.Ascending)
            };

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageOrdersToLinq(orderByAsc), 
                $"{Environment.NewLine}.OrderBy(ord => ord.Name)");
        }

        [TestMethod]
        public void ShouldGetSingleOrderByDesc()
        {
            var orderByDesc = new List<OrderExpression>() {
                new OrderExpression("name", OrderType.Descending)
            };

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageOrdersToLinq(orderByDesc),
                $"{Environment.NewLine}.OrderByDescending(ord => ord.Name)");
        }

        [TestMethod]
        public void ShouldGetMultipleOrderByAsc()
        {
            var orderByAsc = new List<OrderExpression>() {
                new OrderExpression("name", OrderType.Ascending),
                new OrderExpression("opportunityid", OrderType.Ascending),
            };

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageOrdersToLinq(orderByAsc),
                $"{Environment.NewLine}.OrderBy(ord => ord.Name).ThenBy(ord => ord.OpportunityId)");
        }

        [TestMethod]
        public void ShouldGetMultipleOrderByDesc()
        {
            var orderByDesc = new List<OrderExpression>() {
                new OrderExpression("name", OrderType.Descending),
                new OrderExpression("opportunityid", OrderType.Descending)
            };

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageOrdersToLinq(orderByDesc),
                $"{Environment.NewLine}.OrderByDescending(ord => ord.Name).ThenByDescending(ord => ord.OpportunityId)");
        }

        [TestMethod]
        public void ShouldGetMultipleOrderByMix()
        {
            var orderByDesc = new List<OrderExpression>() {
                new OrderExpression("name", OrderType.Descending),
                new OrderExpression("opportunityid", OrderType.Ascending)
            };

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageOrdersToLinq(orderByDesc),
                $"{Environment.NewLine}.OrderByDescending(ord => ord.Name).ThenBy(ord => ord.OpportunityId)");
        }

        [TestMethod]
        public void ShouldGetTopCountNull()
        {
            Assert.AreEqual(converterHelper.queryExpressionTo.ManageTopCountToLinq(null), "");
        }

        [TestMethod]
        public void ShouldGetTopCount()
        {
            Assert.AreEqual(converterHelper.queryExpressionTo.ManageTopCountToLinq(5), $"{Environment.NewLine}.Take(5)");
        }

        [TestMethod]
        public void ShouldGetSingleCondition()
        {
            var criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("name", ConditionOperator.Equal, "test"),
                }
            };

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageCriteriaLinq(criteria), 
                $"{Environment.NewLine}.Where(w => (w.Name == \"test\"))");
        }

        [TestMethod]
        public void ShouldGetMultipleConditions()
        {
            var criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("name", ConditionOperator.Equal, "test"),
                    new ConditionExpression("opportunityid", ConditionOperator.Equal, "guid"),
                    new ConditionExpression("parentaccountid", ConditionOperator.Equal, "guid"),
                }
            };

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageCriteriaLinq(criteria),
                $"{Environment.NewLine}.Where(w => (w.Name == \"test\" && w.OpportunityId == \"guid\" && w.ParentAccountId.Id.ToString() == \"guid\"))");
        }

        [TestMethod]
        public void ShouldGetMultipleAndConditions()
        {
            var criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("name", ConditionOperator.Equal, "test"),
                    new ConditionExpression("opportunityid", ConditionOperator.Equal, "guid")
                },
                FilterOperator = LogicalOperator.And
            };

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageCriteriaLinq(criteria),
                $"{Environment.NewLine}.Where(w => (w.Name == \"test\" && w.OpportunityId == \"guid\"))");
        }

        [TestMethod]
        public void ShouldGetMultipleOrConditions()
        {
            var criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("name", ConditionOperator.Equal, "test"),
                    new ConditionExpression("opportunityid", ConditionOperator.Equal, "guid"),
                },
                FilterOperator = LogicalOperator.Or
            };

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageCriteriaLinq(criteria),
                $"{Environment.NewLine}.Where(w => (w.Name == \"test\" || w.OpportunityId == \"guid\"))");
        }

        [TestMethod]
        public void ShouldGetMultipleAndOrConditions()
        {
            QueryExpression query = new QueryExpression()
            {
                EntityName = "contact",
                Distinct = false,
                ColumnSet = new ColumnSet("fullname", "address1_telephone1"),
                Criteria = {
                    Filters = {
                        new FilterExpression {
                            FilterOperator = LogicalOperator.And,
                            Conditions = {
                                new ConditionExpression("name", ConditionOperator.Equal, "test"),
                                new ConditionExpression("opportunityid", ConditionOperator.Equal, "guid"),
                            }
                        },
                        new FilterExpression {
                            FilterOperator = LogicalOperator.Or,
                            Conditions = {
                                new ConditionExpression("name", ConditionOperator.Equal, "test"),
                                new ConditionExpression("opportunityid", ConditionOperator.Equal, "guid"),
                            }
                        }
                    }
                }
            };

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageCriteriaLinq(query.Criteria),
                $"{Environment.NewLine}.Where(w => (w.Name == \"test\" && w.OpportunityId == \"guid\") && (w.Name == \"test\" || w.OpportunityId == \"guid\"))");
        }
    }
}
