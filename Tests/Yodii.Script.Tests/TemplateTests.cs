#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Script.Tests\WithGlobalContext.cs) is part of Yodii-Script. 
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
using NUnit.Framework;
using FluentAssertions;
using System.Text.RegularExpressions;

namespace Yodii.Script.Tests
{

    [TestFixture]
    public class TemplateTests
    {
        [Test]
        public void foreach_with_one_write_statement()
        {
            var c = new GlobalContext();
            c.Register( "TheList", new[] { 1, 2, 7, 10, 16 } );
            var e = new TemplateEngine( c );
            var r = e.Process( "<%foreach i in TheList {%>*<%=i%><%}%>" );
            r.ErrorMessage.Should().BeNull();
            r.Script.Should().NotBeNull();
            r.Text.Should().Be( "*1*2*7*10*16" );
        }

        [Test]
        public void empty_tags_are_ignored()
        {
            var e = new TemplateEngine( new GlobalContext() );
            {
                var r = e.Process( "<%%>*<%=%>$<%%>" );
                r.ErrorMessage.Should().BeNull();
                r.Script.Should().NotBeNull();
                r.Text.Should().Be( "*$" );
            }
            {
                var r = e.Process( "<% %>*<%= %>$<% %>" );
                r.ErrorMessage.Should().BeNull();
                r.Script.Should().NotBeNull();
                r.Text.Should().Be( "*$" );
            }
        }

        [Test]
        public void when_there_is_no_tag_there_is_no_script()
        {
            var e = new TemplateEngine( new GlobalContext() );
            var r = e.Process( "There is no tag here." );
            r.ErrorMessage.Should().BeNull();
            r.Script.Should().BeNull();
            r.Text.Should().Be( "There is no tag here." );
        }

        class Column
        {
            public string Name { get; set; }

            public string Type { get; set; }

            public bool IsPrimaryKey { get; set; }
        }

        class Table
        {
            public List<Column> Columns { get; set; }
            public string Schema { get; set; }

            public string TableName { get; set; }
        }

        [Test]
        public void simple_template_from_Model_object()
        {
            var t = new Table()
            {
                Schema = "CK",
                TableName = "tToto",
                Columns = new List<Column>()
                {
                    new Column() { Name = "Id", Type = "int", IsPrimaryKey = true },
                    new Column() { Name = "Name", Type = "varchar(40)" }
                }
            };
            var c = new GlobalContext();
            c.Register( "Model", t );
            var e = new TemplateEngine( c );
            var r = e.Process(
                @"create table <%=Model.Schema%>.<%=Model.TableName%> (<%foreach c in Model.Columns {%>
<%=c.Name%> <%=c.Type%><%if c.IsPrimaryKey {%> primary key<%} if( c.$index < Model.Columns.Count - 1) {%>,<%}}%>
);" );
            r.ErrorMessage.Should().BeNull();
            r.Script.Should().NotBeNull();
            r.Text.Should().Be( @"create table CK.tToto (
Id int primary key,
Name varchar(40)
);" );
        }

        [Test]
        public void write_to_template_can_be_transformed_thanks_to_SetWriteTransform()
        {
            var c = new GlobalContext();
            c.Register( "Model", new[] { "This", "will", "be", "in uppercase" } );
            var e = new TemplateEngine( c );
            e.SetWriteTransform( ( s, sb ) => sb.Append( s.ToUpperInvariant() ) );
            var r = e.Process( "<%foreach txt in Model {%> - <%=txt%><%}%> like '<% $writer.Write( 'this' ) %>' but not <% $writer.WriteRaw( 'using WriteRaw' ) %>." );
            r.ErrorMessage.Should().BeNull();
            r.Script.Should().NotBeNull();
            r.Text.Should().Be( " - THIS - WILL - BE - IN UPPERCASE like 'THIS' but not using WriteRaw." );
        }


        [Test]
        public void template_with_WriteRaw()
        {
            var c = new GlobalContext();
            c.Register( "Model", new[] { "This", "will", "be", "in uppercase" } );
            var e = new TemplateEngine( c );
            e.SetWriteTransform( ( s, sb ) => sb.Append( s.ToUpperInvariant() ) );
            var r = e.Process( "<%= 'a' + Model.Length %>|<% $writer.WriteRaw( 'a' + Model.Length ) %>." );
            r.ErrorMessage.Should().BeNull();
            r.Script.Should().NotBeNull();
            r.Text.Should().Be( "A4|a4." );
        }

        public class ModelWithMethod
        {
            public string TheVariable { get; set; }

            public string Reverse( string variable )
            {
                return String.Join( "", variable.Reverse() );
            }
        }

        [Test]
        public void Model_with_property_and_helper_method()
        {
            var model = new ModelWithMethod()
            {
                TheVariable = "azerty"
            };

            var c = new GlobalContext();
            c.Register( "Model", model );

            var e = new TemplateEngine( c );
            var result = e.Process( "Hello, <%= Model.Reverse( Model.TheVariable ) %>" );

            result.Text.Should().Be( $"Hello, {model.Reverse( model.TheVariable )}" );
        }

        [Test]
        public void Model_with_property_and_helper_method_contains_in_a_foreach()
        {
            var model = new List<ModelWithMethod>()
            {
                new ModelWithMethod() {  TheVariable = "First" },
                new ModelWithMethod() {  TheVariable = "Second" },
            };

            var c = new GlobalContext();
            c.Register( "Model", model );

            var e = new TemplateEngine( c );
            var result = e.Process( @"Hello,
                <%
                    foreach( m in Model ) {
                %>
                    <%= m.Reverse( m.TheVariable ) %>
                <%
                    }
                %>" );

            result.ErrorMessage.Should().BeNullOrEmpty();

            string content = Regex.Replace( result.Text, @"\s+", string.Empty );

            content.Should().Be( $"Hello,{model[0].Reverse( model[0].TheVariable )}{model[1].Reverse( model[1].TheVariable )}" );
        }

        [Test]
        public void Model_with_property_and_helper_method_contains_in_a_foreach_with_temp_variable()
        {
            var model = new List<ModelWithMethod>()
            {
                new ModelWithMethod() {  TheVariable = "First" },
                new ModelWithMethod() {  TheVariable = "Seacond" },
            };

            var c = new GlobalContext();
            c.Register( "Model", model );

            var e = new TemplateEngine( c );
            var result = e.Process( @"Hello,
                <%
                    foreach( m in Model ) {
                        let tmp = m.TheVariable;
                %>
                    <%= m.Reverse( tmp ) %>
                <%
                    }
                %>" );

            result.ErrorMessage.Should().BeNullOrEmpty();

            string content = Regex.Replace( result.Text, @"\s+", string.Empty );

            content.Should().Be( $"Hello,{model[0].Reverse( model[0].TheVariable )}{model[1].Reverse( model[1].TheVariable )}" );
        }


    }
}
