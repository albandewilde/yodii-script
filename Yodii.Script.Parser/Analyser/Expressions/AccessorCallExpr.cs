#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Script\Analyser\Expressions\AccessorCallExpr.cs) is part of Yodii-Script. 
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Yodii.Script
{
    public class AccessorCallExpr : AccessorExpr
    {
        readonly IReadOnlyList<Expr> _args;
        readonly IReadOnlyList<AccessorLetExpr> _declaredFunctions;

        /// <summary>
        /// Creates a new <see cref="AccessorCallExpr"/>: 0 or n arguments can be provided.
        /// </summary>
        /// <param name="location">Source location.</param>
        /// <param name="left">Left scope. Must not be null.</param>
        /// <param name="arguments">When null, it is normalized to <see cref="Expr.EmptyArray"/>.</param>
        /// <param name="declaredFunctions">Declared functions in the call itself (ie. named callback functions). Can be null.</param>
        /// <param name="isIndexer">True if this is an indexer[] instead of a regular call().</param>
        /// <param name="isStatement">True for statement, false for expression.</param>
        public AccessorCallExpr( SourceLocation location, Expr left, IReadOnlyList<Expr> arguments, IReadOnlyList<AccessorLetExpr> declaredFunctions, bool isStatement, bool isIndexer )
            : base( location, left, isStatement, true )
        {
            _args = arguments ?? Expr.EmptyArray;
            _declaredFunctions = declaredFunctions;
            IsIndexer = isIndexer;
        }

        /// <summary>
        /// Gets the list of arguments.
        /// </summary>
        public override IReadOnlyList<Expr> Arguments => _args;

        /// <summary>
        /// Gets the declared functions in the call itself (ie. named callback functions). 
        /// Can be null.
        /// </summary>
        public IReadOnlyList<AccessorLetExpr> DeclaredFunctions => _declaredFunctions;

        /// <summary>
        /// Gets whether this call is actually an index: [] instead of ().
        /// </summary>
        public bool IsIndexer { get; }

        /// <summary>
        /// Parametrized implementation of the visitor's double dispatch.
        /// </summary>
        /// <typeparam name="T">Type of the visitor's returned data.</typeparam>
        /// <param name="visitor">visitor.</param>
        /// <returns>The result of the visit.</returns>
        [DebuggerStepThrough]
        public override T Accept<T>( IExprVisitor<T> visitor ) => visitor.Visit( this );

        /// <summary>
        /// This is just to ease debugging.
        /// </summary>
        /// <returns>Readable expression.</returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder( Left.ToString() );
            b.Append( IsIndexer ? '[' : '(' );
            bool first = true;
            foreach( var e in Arguments )
            {
                if( first ) first = false;
                else b.Append( ',' );
                b.Append( e.ToString() );
            }
            b.Append( IsIndexer ? ']' : ')' );
            return b.ToString();
        }
    }

}
