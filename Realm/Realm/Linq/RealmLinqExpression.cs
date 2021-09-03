using System.Linq.Expressions;

namespace Realms
{
    /*
     * Helper class used for storing the entire expression for classes using ExpressionVisitor
     */
    internal class RealmLinqExpression : Expression
    {
        public ExpressionNode ExpressionNode { get; private set; }

        public static RealmLinqExpression Create(ExpressionNode exp)
        {
            return new RealmLinqExpression { ExpressionNode = exp };
        }
    }
}
