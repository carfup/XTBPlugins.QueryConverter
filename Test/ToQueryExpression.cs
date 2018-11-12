using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Carfup.XTBPlugins.QueryConverter.AppCode;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using Microsoft.Xrm.Tooling.Connector;
using Test;

namespace Carfup.XTBPlugins.QueryConverter.Test
{
    [TestClass]
    public class ToQueryExpression
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
        public void ShouldGetAllColumnsFromWebApi()
        {
            var columns = "";
            var columnsStar = "*";
            string columnsNull = null;

            Assert.AreEqual(converterHelper.webApiTo.ManageColumns(columns), "ColumnSet = new ColumnSet(true)");
            Assert.AreEqual(converterHelper.webApiTo.ManageColumns(columnsStar), "ColumnSet = new ColumnSet(true)");
            Assert.AreEqual(converterHelper.webApiTo.ManageColumns(columnsNull), "ColumnSet = new ColumnSet(true)");
        }

        [TestMethod]
        public void ShouldGetMultipleColumns()
        {
            var columns = "name, opportunityid";

            Assert.AreEqual(converterHelper.webApiTo.ManageColumns(columns), 
                "ColumnSet = new ColumnSet(\"name\",\"opportunityid\")");
        }

        [TestMethod]
        public void ShouldGetSingleOrderByAsc()
        {
            var orderByAsc = "orderby = name asc";

            Assert.AreEqual(converterHelper.webApiTo.ManagerOrders(orderByAsc), 
                "Orders = { new OrderExpression(\"name\", OrderType.Ascending)}");
        }

        [TestMethod]
        public void ShouldGetSingleOrderByDesc()
        {
            var orderByAsc = "orderby = name desc";

            Assert.AreEqual(converterHelper.webApiTo.ManagerOrders(orderByAsc), 
                "Orders = { new OrderExpression(\"name\", OrderType.Descending)}");
        }

        [TestMethod]
        public void ShouldGetMultipleOrderByAsc()
        {
            var orderByAsc = "orderby=name asc,opportunityid asc";

            Assert.AreEqual(converterHelper.webApiTo.ManagerOrders(orderByAsc), 
                "Orders = {new OrderExpression(\"name\", OrderType.Ascending),new OrderExpression(\"opportunityid\", OrderType.Ascending)}");
        }

        [TestMethod]
        public void ShouldGetMultipleOrderByDesc()
        {
            var orderByDesc = "orderby=name desc,opportunityid desc";

            Assert.AreEqual(converterHelper.webApiTo.ManagerOrders(orderByDesc),
                "Orders = {new OrderExpression(\"name\", OrderType.Descending),new OrderExpression(\"opportunityid\", OrderType.Descending)}");
        }

        [TestMethod]
        public void ShouldGetMultipleOrderByMix()
        {
            var orderByDesc = "orderby=name asc,opportunityid desc";

            Assert.AreEqual(converterHelper.webApiTo.ManagerOrders(orderByDesc),
                "Orders = {new OrderExpression(\"name\", OrderType.Ascending),new OrderExpression(\"opportunityid\", OrderType.Descending)}");
        }

        [TestMethod]
        public void ShouldGetTopCountNull()
        {
            Assert.AreEqual(converterHelper.webApiTo.ManageTopCount(null), "");
            Assert.AreEqual(converterHelper.webApiTo.ManageTopCount(""), "");
        }

        [TestMethod]
        public void ShouldGetTopCount()
        {
            Assert.AreEqual(converterHelper.webApiTo.ManageTopCount("5"), "TopCount = 5");
        }


        /// <summary>
        ///  TO COMPLETE FROM HERE
        /// </summary>
        [TestMethod]
        public void ShouldGetSingleCondition()
        {

            var criteria = "filter = (name eq 'test')";
            
            Assert.AreEqual(converterHelper.webApiTo.ManageFilters(criteria), "$filter=(name eq 'test')");
        }

        [TestMethod]
        public void ShouldGetMultipleConditions()
        {
            var criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("name", ConditionOperator.Equal, "test"),
                    new ConditionExpression("opportunityid", ConditionOperator.Equal, "guid")
                }
            };

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageConditionsToWebApi(criteria, ""), "$filter=(name eq 'test' and opportunityid eq 'guid')");
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

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageConditionsToWebApi(criteria, ""), "$filter=(name eq 'test' and opportunityid eq 'guid')");
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

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageConditionsToWebApi(criteria, ""), "$filter=(name eq 'test' or opportunityid eq 'guid')");
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

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageConditionsToWebApi(query.Criteria, ""), "$filter=(name eq 'test' and opportunityid eq 'guid') and (name eq 'test' or opportunityid eq 'guid')");
        }
    }
}
