using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Carfup.XTBPlugins.QueryConverter.AppCode;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

namespace Carfup.XTBPlugins.Test
{
    [TestClass]
    public class ToWebApi
    {
       ConverterHelper converterHelper = new ConverterHelper(null);

        [TestMethod]
        public void ShouldGetAllColumns()
        {
            var columnSet = new ColumnSet(true);

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageColumsetToWebApi(columnSet), "");
        }

        [TestMethod]
        public void ShouldGetMultipleColumns()
        {
            var columnSet = new ColumnSet("name", "opportunityid");

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageColumsetToWebApi(columnSet), "$select=name,opportunityid");
        }

        [TestMethod]
        public void ShouldGetSingleOrderByAsc()
        {
            var orderByAsc = new List<OrderExpression>() {
                new OrderExpression("name", OrderType.Ascending)
            };

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageOrdersToWebApi(orderByAsc), "$orderby=name asc");
        }

        [TestMethod]
        public void ShouldGetSingleOrderByDesc()
        {
            var orderByDesc = new List<OrderExpression>() {
                new OrderExpression("name", OrderType.Descending)
            };

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageOrdersToWebApi(orderByDesc), "$orderby=name desc");
        }

        [TestMethod]
        public void ShouldGetMultipleOrderByAsc()
        {
            var orderByAsc = new List<OrderExpression>() {
                new OrderExpression("name", OrderType.Ascending),
                new OrderExpression("opportunityid", OrderType.Ascending),
            };

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageOrdersToWebApi(orderByAsc), "$orderby=name asc,opportunityid asc");
        }

        [TestMethod]
        public void ShouldGetMultipleOrderByDesc()
        {
            var orderByDesc = new List<OrderExpression>() {
                new OrderExpression("name", OrderType.Descending),
                new OrderExpression("opportunityid", OrderType.Descending)
            };

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageOrdersToWebApi(orderByDesc), "$orderby=name desc,opportunityid desc");
        }

        [TestMethod]
        public void ShouldGetMultipleOrderByMix()
        {
            var orderByDesc = new List<OrderExpression>() {
                new OrderExpression("name", OrderType.Descending),
                new OrderExpression("opportunityid", OrderType.Ascending)
            };

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageOrdersToWebApi(orderByDesc), "$orderby=name desc,opportunityid asc");
        }

        [TestMethod]
        public void ShouldGetTopCountNull()
        {
            Assert.AreEqual(converterHelper.queryExpressionTo.ManageTopCountToWebApi(null), "");
        }

        [TestMethod]
        public void ShouldGetTopCount()
        {
            Assert.AreEqual(converterHelper.queryExpressionTo.ManageTopCountToWebApi(5), "$top=5");
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

            Assert.AreEqual(converterHelper.queryExpressionTo.ManageConditionsToWebApi(criteria, ""), "$filter=(name eq 'test')");
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
