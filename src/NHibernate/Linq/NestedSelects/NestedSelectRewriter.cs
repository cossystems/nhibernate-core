using System.Linq;
using System.Linq.Expressions;
using NHibernate.Linq.GroupBy;
using Remotion.Linq;

namespace NHibernate.Linq.NestedSelects
{
	static class NestedSelectRewriter
	{
		public static void ReWrite(QueryModel queryModel, ISessionFactory sessionFactory)
		{
			var nsqmv = new NestedSelectDetector(sessionFactory);
			nsqmv.Visit(queryModel.SelectClause.Selector);
			if (!nsqmv.HasSubqueries)
				return;

			var rewriter = new NestedSelectClauseRewriter(sessionFactory, queryModel);
			var expression = rewriter.Start();
			var initializers = rewriter.Expressions.Select(ConvertToObject);
			queryModel.ResultOperators.Add(new ClientSideSelect2((LambdaExpression) expression));
			queryModel.SelectClause.Selector = Expression.NewArrayInit(typeof(object), initializers);
		}

		private static Expression ConvertToObject(Expression expression)
		{
			return Expression.Convert(expression, typeof (object));
		}
	}
}
