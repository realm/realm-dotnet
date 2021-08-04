using System.Linq.Expressions;

namespace Realms
{
    internal class RealmLinqExpression : Expression
    {
        public ExpressionNode ExpressionNode { get; private set; }

        public static RealmLinqExpression Create(ExpressionNode exp)
        {
            return new RealmLinqExpression { ExpressionNode = exp };
        }
    }
}
