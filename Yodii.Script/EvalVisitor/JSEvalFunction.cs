#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Script\EvalVisitor\JSEvalFunction.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Yodii.Script
{
    public class JSEvalFunction : RuntimeObj
    {
        readonly FunctionExpr _expr;
        readonly IReadOnlyList<Closure> _closures;

        internal JSEvalFunction( FunctionExpr e, IReadOnlyList<Closure> closures )
        {
            if( e == null ) throw new ArgumentNullException();
            _expr = e;
            _closures = closures;
        }

        public FunctionExpr Expr 
        { 
            get { return _expr; } 
        }

        public override string Type
        {
            get { return RuntimeObj.TypeObject; }
        }

        public override bool ToBoolean()
        {
            return true;
        }

        public override double ToDouble()
        {
            return Double.NaN;
        }

        public override string ToString()
        {
            return _expr.ToString();
        }

        public override PExpr Visit( IAccessorFrame frame )
        {
            if( frame.Expr is AccessorCallExpr )
            {
                return new EvalVisitor.FunctionExprFrame( (EvalVisitor.AccessorFrame)frame, _expr, _closures ).Visit();
            }
            return base.Visit( frame );
        }
    }
}