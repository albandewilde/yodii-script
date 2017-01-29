#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Script.Tests\BasicBreakpointSupport.cs) is part of Yodii-Script. 
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
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace Yodii.Script.Tests
{
    
    public class BasicBreakpointSupport
    {
        [Theory]
        [InlineData( "3" )]
        [InlineData( "3+7" )]
        [InlineData( "5 < 8" )]
        [InlineData( "(5&2) <= (7-(4<<2)*5+69)" )]
        public void breaking_and_restarting_an_evaluation( string s )
        {
            ScriptEngine engine = new ScriptEngine();
            Expr e = Analyzer.AnalyseString( s );
            RuntimeObj syncResult;
            using( var r1 = engine.Execute( e ) )
            {
                r1.Status.Should().Be( ScriptEngineStatus.IsFinished );
                syncResult = r1.CurrentResult;
            }
            engine.Breakpoints.BreakAlways = true;
            using( var r2 = engine.Execute( e ) )
            {
                int nbStep = 0;
                while( r2.CanContinue )
                {
                    ++nbStep;
                    r2.Continue();
                }
                r2.Status.Should().Be( ScriptEngineStatus.IsFinished );
                r2.CurrentResult.ToString().Should().Be( syncResult.ToString() );
                Console.WriteLine( "String '{0}' = {1} evaluated in {2} steps.", s, syncResult.ToString(), nbStep );
            }
        }
    }
}
