#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Script.Tests\JSAnalyserTests.cs) is part of Yodii-Script. 
*  
* Yodii-Script is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* Yodii-Script is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with Yodii-Script. If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>, IN'TECH INFO <http://www.intechinfo.fr>
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Yodii.Script;
using CK.Core;

namespace Yodii.Script.Tests
{
    [TestFixture]
    public class AnalyserTests
    {
        [Test]
        public void an_empty_string_is_a_syntax_error()
        {
            ExprAnalyser a = new ExprAnalyser();
            JSTokenizer p = new JSTokenizer();
            {
                p.Reset( "" );
                Assert.That( p.IsEndOfInput );
                Expr e = a.Analyse( p );
                Assert.That( e is SyntaxErrorExpr );
            }
            {
                p.Reset( " \r\n \n   \r  \n \t  " );
                Assert.That( p.IsEndOfInput );
                Expr e = a.Analyse( p );
                Assert.That( e is SyntaxErrorExpr );
            }
        }

        [Test]
        public void SimpleExpression()
        {
            ExprAnalyser a = new ExprAnalyser();
            JSTokenizer p = new JSTokenizer();

            {
                p.Reset( "value" );
                Assert.That( p.IsErrorOrEndOfInput, Is.False );
                 Expr e = a.Analyse( p );
                Assert.That( e is AccessorMemberExpr );
                AccessorMemberExpr ac = e as AccessorMemberExpr;
                Assert.That( ac.IsUnbound == true );
            }
            {
                p.Reset( "!" );
                Expr e = a.Analyse( p );
                Assert.That( e is UnaryExpr );
                UnaryExpr u = e as UnaryExpr;
                Assert.That( u.TokenType == JSTokenizerToken.Not );
                Assert.That( u.Expression is SyntaxErrorExpr );
                Assert.That( SyntaxErrorCollector.Collect( e, null ).Count == 1 );
            }
            {
                p.Reset( "!value" );
                Expr e = a.Analyse( p );
                Assert.That( e is UnaryExpr );
                UnaryExpr u = e as UnaryExpr;
                Assert.That( u.TokenType == JSTokenizerToken.Not );
                Assert.That( u.Expression is AccessorExpr );
                Assert.That( SyntaxErrorCollector.Collect( e, Util.ActionVoid ).Count == 0 );
            }
            {
                p.Reset( " 0.12e43 && ~b " );
                Expr e = a.Analyse( p );
                Assert.That( e is BinaryExpr );
                BinaryExpr and = e as BinaryExpr;
                Assert.That( and.BinaryOperatorToken == JSTokenizerToken.And );
                IsConstant( and.Left, 0.12e43 );
                Assert.That( and.Right is UnaryExpr );
                UnaryExpr u = and.Right as UnaryExpr;
                Assert.That( u.TokenType == JSTokenizerToken.BitwiseNot );
                Assert.That( u.Expression is AccessorExpr );

                Assert.That( SyntaxErrorCollector.Collect( e, Util.ActionVoid ).Count == 0 );
            }
            {
                p.Reset( @"!a||~""x""" );
                Expr e = a.Analyse( p );
                Assert.That( e is BinaryExpr );
                BinaryExpr or = e as BinaryExpr;
                Assert.That( or.BinaryOperatorToken == JSTokenizerToken.Or );
                Assert.That( or.Left is UnaryExpr );
                Assert.That( or.Right is UnaryExpr );
                UnaryExpr u = or.Right as UnaryExpr;
                Assert.That( u.TokenType == JSTokenizerToken.BitwiseNot );
                IsConstant( u.Expression, "x" );

                Assert.That( SyntaxErrorCollector.Collect( e, Util.ActionVoid ).Count == 0 );
            }
            {
                p.Reset( "(3)" );
                Expr e = a.Analyse( p );
                IsConstant( e, 3 );
            }
            {
                p.Reset( "(3+typeof 'x')" );
                Expr e = a.Analyse( p );
                Assert.That( e is BinaryExpr );
                BinaryExpr b = e as BinaryExpr;
                IsConstant( b.Left, 3 );
                Assert.That( b.Right is UnaryExpr );
                UnaryExpr u = b.Right as UnaryExpr;
                Assert.That( u.TokenType == JSTokenizerToken.TypeOf );
                IsConstant( u.Expression, "x" );

                Assert.That( SyntaxErrorCollector.Collect( e, Util.ActionVoid ).Count == 0 );
            }
            {
                p.Reset( "1 ? 2 : 3" );
                Expr e = a.Analyse( p );
                Assert.That( e is IfExpr );
                IfExpr i = e as IfExpr;
                Assert.That( i.IsTernaryOperator == true );
                IsConstant( i.Condition, 1 );
                IsConstant( i.WhenTrue, 2 );
                IsConstant( i.WhenFalse, 3 );
            }
        }

        [Test]
        public void ArraySupport()
        {
            ExprAnalyser a = new ExprAnalyser();
            JSTokenizer p = new JSTokenizer();
            {
                p.Reset( "a[9]" );
                Assert.That( p.IsErrorOrEndOfInput, Is.False );
                Expr e = a.Analyse( p );
                Assert.That( e is AccessorCallExpr );
                AccessorCallExpr ac = e as AccessorCallExpr;
                IsConstant( ac.Arguments[0], 9 );
            }
            {
                p.Reset( "array['Hello World!','H']" );
                Assert.That( p.IsErrorOrEndOfInput, Is.False );
                Expr e = a.Analyse( p );
                Assert.That( e is AccessorCallExpr );
                AccessorCallExpr ac = e as AccessorCallExpr;
                IsConstant( ac.Arguments[0], "Hello World!" );
                IsConstant( ac.Arguments[1], "H" );
            }
        }

        void IsConstant( Expr e, object o )
        {
            Assert.That( e is ConstantExpr );
            ConstantExpr c = e as ConstantExpr;
            Assert.That( c.Value, Is.EqualTo( o ) );
        }
    }
}
