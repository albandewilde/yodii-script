#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Script\EvalVisitor\JSEvalFunction.cs) is part of Yodii-Script. 
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


namespace Yodii.Script
{
    public class FunctionObj : RuntimeObj
    {
        readonly FunctionExpr _expr;
        readonly IReadOnlyList<Closure> _closures;

        internal FunctionObj( FunctionExpr e, IReadOnlyList<Closure> closures )
        {
            if( e == null ) throw new ArgumentNullException();
            _expr = e;
            _closures = closures;
        }

        public FunctionExpr Expr  => _expr; 

        public override string Type => RuntimeObj.TypeFunction;

        public override object ToNative( GlobalContext c ) => null;

        public override bool ToBoolean() => true;

        public override double ToDouble() => double.NaN;

        public override string ToString() => _expr.ToString();

        public override PExpr Visit( IAccessorFrame frame )
        {
            if( frame.Expr is AccessorCallExpr )
            {
                EvalVisitor.AccessorFrame f = (EvalVisitor.AccessorFrame)frame;
                return f.Visitor.Run( new EvalVisitor.FunctionExprFrame( f, _expr, _closures ) );
            }
            return frame.SetError();
        }
    }
}
