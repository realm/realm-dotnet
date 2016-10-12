////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Realms
{
    internal abstract class ExpressionVisitor
    {
        internal virtual Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return exp;
            }

            switch (exp.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.ArrayIndex:
                case ExpressionType.Coalesce:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LeftShift:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.NotEqual:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Power:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return this.VisitBinary((BinaryExpression)exp);
                case ExpressionType.ArrayLength:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Negate:
                case ExpressionType.UnaryPlus:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return this.VisitUnary((UnaryExpression)exp);
                case ExpressionType.Call:
                    return this.VisitMethodCall((MethodCallExpression)exp);
                case ExpressionType.Conditional:
                    return this.VisitConditional((ConditionalExpression)exp);
                case ExpressionType.Constant:
                    return this.VisitConstant((ConstantExpression)exp);
                case ExpressionType.Invoke:
                    return this.VisitInvocation((InvocationExpression)exp);
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                case ExpressionType.ListInit:
                    return this.VisitListInit((ListInitExpression)exp);
                case ExpressionType.MemberAccess:
                    return this.VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.MemberInit:
                    return this.VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.New:
                    return (Expression)this.VisitNew((NewExpression)exp);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return this.VisitNewArray((NewArrayExpression)exp);
                case ExpressionType.Parameter:
                    return this.VisitParameter((ParameterExpression)exp);
                case ExpressionType.TypeIs:
                    return this.VisitTypeIs((TypeBinaryExpression)exp);
                default:
                    throw new ArgumentException(exp.NodeType.ToString());
            }
        }

        internal virtual MemberBinding VisitBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return this.VisitMemberAssignment((MemberAssignment)binding);
                case MemberBindingType.MemberBinding:
                    return this.VisitMemberMemberBinding((MemberMemberBinding)binding);
                case MemberBindingType.ListBinding:
                    return this.VisitMemberListBinding((MemberListBinding)binding);
                default:
                    throw new ArgumentException(binding.BindingType.ToString());
            }
        }

        internal virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            var readOnlyCollection = this.VisitExpressionList(initializer.Arguments);
            if (readOnlyCollection != initializer.Arguments)
            {
                return Expression.ElementInit(initializer.AddMethod, readOnlyCollection);
            }

            return initializer;
        }

        internal virtual Expression VisitUnary(UnaryExpression u)
        {
            var operand = this.Visit(u.Operand);
            if (operand != u.Operand)
            {
                return Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);
            }

            return u;
        }

        internal virtual Expression VisitBinary(BinaryExpression b)
        {
            var left = this.Visit(b.Left);
            var right = this.Visit(b.Right);
            var expression = this.Visit(b.Conversion);
            if (left == b.Left && right == b.Right && expression == b.Conversion)
            {
                return b;
            }

            if (b.NodeType == ExpressionType.Coalesce && b.Conversion != null)
            {
                return Expression.Coalesce(left, right, expression as LambdaExpression);
            }

            return Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
        }

        internal virtual Expression VisitTypeIs(TypeBinaryExpression b)
        {
            var expression = this.Visit(b.Expression);
            if (expression != b.Expression)
            {
                return Expression.TypeIs(expression, b.TypeOperand);
            }

            return b;
        }

        internal virtual Expression VisitConstant(ConstantExpression c) => c;

        internal virtual Expression VisitConditional(ConditionalExpression c)
        {
            var test = this.Visit(c.Test);
            var ifTrue = this.Visit(c.IfTrue);
            var ifFalse = this.Visit(c.IfFalse);
            if (test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse)
            {
                return Expression.Condition(test, ifTrue, ifFalse);
            }

            return c;
        }

        internal virtual Expression VisitParameter(ParameterExpression p) => p;

        internal virtual Expression VisitMemberAccess(MemberExpression m)
        {
            var expression = this.Visit(m.Expression);
            if (expression != m.Expression)
            {
                return Expression.MakeMemberAccess(expression, m.Member);
            }

            return m;
        }

        internal virtual Expression VisitMethodCall(MethodCallExpression m)
        {
            var instance = this.Visit(m.Object);
            var arguments = this.VisitExpressionList(m.Arguments);
            if (instance != m.Object || arguments != m.Arguments)
            {
                return Expression.Call(instance, m.Method, arguments);
            }

            return m;
        }

        internal virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<Expression> list = null;
            var count = original.Count;
            for (var index1 = 0; index1 < count; ++index1)
            {
                var expression = this.Visit(original[index1]);
                if (list != null)
                {
                    list.Add(expression);
                }
                else if (expression != original[index1])
                {
                    list = new List<Expression>(count);
                    for (int index2 = 0; index2 < index1; ++index2)
                    {
                        list.Add(original[index2]);
                    }

                    list.Add(expression);
                }
            }

            if (list != null)
            {
                return list.ToReadOnlyCollection();
            }

            return original;
        }

        internal virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            var expression = this.Visit(assignment.Expression);
            if (expression != assignment.Expression)
            {
                return Expression.Bind(assignment.Member, expression);
            }

            return assignment;
        }

        internal virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            var bindings = this.VisitBindingList(binding.Bindings);
            if (bindings != binding.Bindings)
            {
                return Expression.MemberBind(binding.Member, bindings);
            }

            return binding;
        }

        internal virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            var initializers = this.VisitElementInitializerList(binding.Initializers);
            if (initializers != binding.Initializers)
            {
                return Expression.ListBind(binding.Member, initializers);
            }

            return binding;
        }

        internal virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding> list = null;
            var count = original.Count;
            for (var index1 = 0; index1 < count; ++index1)
            {
                var memberBinding = this.VisitBinding(original[index1]);
                if (list != null)
                {
                    list.Add(memberBinding);
                }
                else if (memberBinding != original[index1])
                {
                    list = new List<MemberBinding>(count);
                    for (int index2 = 0; index2 < index1; ++index2)
                    {
                        list.Add(original[index2]);
                    }

                    list.Add(memberBinding);
                }
            }

            return list ?? (IEnumerable<MemberBinding>)original;
        }

        internal virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit> list = null;
            var count = original.Count;
            for (var index1 = 0; index1 < count; ++index1)
            {
                var elementInit = this.VisitElementInitializer(original[index1]);
                if (list != null)
                {
                    list.Add(elementInit);
                }
                else if (elementInit != original[index1])
                {
                    list = new List<ElementInit>(count);
                    for (var index2 = 0; index2 < index1; ++index2)
                    {
                        list.Add(original[index2]);
                    }

                    list.Add(elementInit);
                }
            }

            return list ?? (IEnumerable<ElementInit>)original;
        }

        internal virtual Expression VisitLambda(LambdaExpression lambda)
        {
            var body = this.Visit(lambda.Body);
            if (body != lambda.Body)
            {
                return Expression.Lambda(lambda.Type, body, lambda.Parameters);
            }

            return lambda;
        }

        internal virtual NewExpression VisitNew(NewExpression nex)
        {
            var arguments = this.VisitExpressionList(nex.Arguments);
            if (arguments == nex.Arguments)
            {
                return nex;
            }

            if (nex.Members != null)
            {
                return Expression.New(nex.Constructor, arguments, nex.Members);
            }

            return Expression.New(nex.Constructor, arguments);
        }

        internal virtual Expression VisitMemberInit(MemberInitExpression init)
        {
            var newExpression = this.VisitNew(init.NewExpression);
            var bindings = this.VisitBindingList(init.Bindings);
            if (newExpression != init.NewExpression || bindings != init.Bindings)
            {
                return Expression.MemberInit(newExpression, bindings);
            }

            return init;
        }

        internal virtual Expression VisitListInit(ListInitExpression init)
        {
            var newExpression = this.VisitNew(init.NewExpression);
            var initializers = this.VisitElementInitializerList(init.Initializers);
            if (newExpression != init.NewExpression || initializers != init.Initializers)
            {
                return Expression.ListInit(newExpression, initializers);
            }

            return init;
        }

        internal virtual Expression VisitNewArray(NewArrayExpression na)
        {
            var enumerable = this.VisitExpressionList(na.Expressions);
            if (enumerable == na.Expressions)
            {
                return na;
            }

            if (na.NodeType == ExpressionType.NewArrayInit)
            {
                return Expression.NewArrayInit(na.Type.GetElementType(), enumerable);
            }

            return Expression.NewArrayBounds(na.Type.GetElementType(), enumerable);
        }

        internal virtual Expression VisitInvocation(InvocationExpression iv)
        {
            var arguments = this.VisitExpressionList(iv.Arguments);
            var expression = this.Visit(iv.Expression);
            if (arguments != iv.Arguments || expression != iv.Expression)
            {
                return Expression.Invoke(expression, arguments);
            }

            return iv;
        }
    }
}