using System;
using Carfup.XTBPlugins.QueryConverter.AppCode;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;

namespace Carfup.XTBPlugins.QueryConverter.UnitTest
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
        public void ShouldGetMultipleColumnsFromWebApi()
        {
            var columns = "name, opportunityid";

            Assert.AreEqual(converterHelper.webApiTo.ManageColumns(columns),
                "ColumnSet = new ColumnSet(\"name\",\"opportunityid\")");
        }

        [TestMethod]
        public void ShouldGetSingleOrderByAscFromWebApi()
        {
            var orderByAsc = "orderby=name asc";
            var dunno = orderByAsc.Split('=');

            Assert.AreEqual(converterHelper.webApiTo.ManagerOrders(dunno[1]),
                "Orders = { new OrderExpression(\"name\", OrderType.Ascending)}");
        }

        [TestMethod]
        public void ShouldGetSingleOrderByDescFromWebApi()
        {
            var orderByAsc = "orderby=name desc";
            var dunno = orderByAsc.Split('=');

            Assert.AreEqual(converterHelper.webApiTo.ManagerOrders(dunno[1]),
                "Orders = { new OrderExpression(\"name\", OrderType.Descending)}");
        }

        [TestMethod]
        public void ShouldGetMultipleOrderByAscFromWebApi()
        {
            var orderByAsc = "orderby=name asc,opportunityid asc";
            var dunno = orderByAsc.Split('=');

            Assert.AreEqual(converterHelper.webApiTo.ManagerOrders(dunno[1]),
                "Orders = { new OrderExpression(\"name\", OrderType.Ascending),new OrderExpression(\"opportunityid\", OrderType.Ascending)}");
        }

        [TestMethod]
        public void ShouldGetMultipleOrderByDescFromWebApi()
        {
            var orderByDesc = "orderby=name desc,opportunityid desc";
            var dunno = orderByDesc.Split('=');

            Assert.AreEqual(converterHelper.webApiTo.ManagerOrders(dunno[1]),
                "Orders = { new OrderExpression(\"name\", OrderType.Descending),new OrderExpression(\"opportunityid\", OrderType.Descending)}");
        }

        [TestMethod]
        public void ShouldGetMultipleOrderByMixFromWebApi()
        {
            var orderByDesc = "orderby=name asc,opportunityid desc";
            var dunno = orderByDesc.Split('=');

            Assert.AreEqual(converterHelper.webApiTo.ManagerOrders(dunno[1]),
                "Orders = { new OrderExpression(\"name\", OrderType.Ascending),new OrderExpression(\"opportunityid\", OrderType.Descending)}");
        }

        [TestMethod]
        public void ShouldGetTopCountNullFromWebApi()
        {
            Assert.AreEqual(converterHelper.webApiTo.ManageTopCount(null), "");
            Assert.AreEqual(converterHelper.webApiTo.ManageTopCount(""), "");
        }

        [TestMethod]
        public void ShouldGetTopCountFromWebApi()
        {
            Assert.AreEqual(converterHelper.webApiTo.ManageTopCount("5"), "TopCount = 5");
        }


        /// <summary>
        ///  TO COMPLETE FROM HERE
        /// </summary>
        [TestMethod]
        public void ShouldGetSingleConditionFromWebApi()
        {
            var criteria = "filter=(name eq 'test')";
            var dunno = criteria.Split('=');

            Assert.AreEqual(converterHelper.webApiTo.ManageFilters(dunno[1]),
                "Criteria = { Filters = { new FilterExpression { Conditions = { new ConditionExpression(\"name\", ConditionOperator.Equal, \"test\") }, FilterOperator = LogicalOperator.And}}}");
        }

        [TestMethod]
        public void ShouldGetMultipleConditionsFromWebApi()
        {
            var criteria = "filter=(name eq 'test' and opportunity eq 'guid')";
            var dunno = criteria.Split('=');

            Assert.AreEqual(converterHelper.webApiTo.ManageFilters(dunno[1]),
                "Criteria = { Filters = { new FilterExpression { Conditions = { new ConditionExpression(\"name\", ConditionOperator.Equal, \"test\"),new ConditionExpression(\"opportunity\", ConditionOperator.Equal, \"guid\") }, FilterOperator = LogicalOperator.And}}}");
        }

        [TestMethod]
        public void ShouldGetMultipleAndConditionsFromWebApi()
        {
            var criteria = "filter=(name eq 'test' and opportunity eq 'guid')";
            var dunno = criteria.Split('=');

            Assert.AreEqual(converterHelper.webApiTo.ManageFilters(dunno[1]),
                "Criteria = { Filters = { new FilterExpression { Conditions = { new ConditionExpression(\"name\", ConditionOperator.Equal, \"test\"),new ConditionExpression(\"opportunity\", ConditionOperator.Equal, \"guid\") }, FilterOperator = LogicalOperator.And}}}");
        }

        [TestMethod]
        public void ShouldGetMultipleOrConditionsFromWebApi()
        {
            var criteria = "filter=(name eq 'test' or opportunity eq 'guid')";
            var dunno = criteria.Split('=');

            Assert.AreEqual(converterHelper.webApiTo.ManageFilters(dunno[1]),
                "Criteria = { Filters = { new FilterExpression { Conditions = { new ConditionExpression(\"name\", ConditionOperator.Equal, \"test\"),new ConditionExpression(\"opportunity\", ConditionOperator.Equal, \"guid\") }, FilterOperator = LogicalOperator.Or}}}");
        }

        [TestMethod]
        public void ShouldGetMultipleAndOrConditionsFromWebApi()
        {
            var criteria = "filter=(name eq 'test' and opportunity eq 'guid') and (name eq 'test' or opportunity eq 'guid')";
            var dunno = criteria.Split('=');

            Assert.AreEqual(converterHelper.webApiTo.ManageFilters(dunno[1]),
                "Criteria = { Filters = { new FilterExpression { Conditions = { new ConditionExpression(\"name\", ConditionOperator.Equal, \"test\"),new ConditionExpression(\"opportunity\", ConditionOperator.Equal, \"guid\") }, FilterOperator = LogicalOperator.And}, new FilterExpression { Conditions = { new ConditionExpression(\"name\", ConditionOperator.Equal, \"test\"),new ConditionExpression(\"opportunity\", ConditionOperator.Equal, \"guid\") }, FilterOperator = LogicalOperator.Or}}}");
        }
    }
}
