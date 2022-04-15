using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class HtmlBuilder
    {
        private StringBuilder sb = new StringBuilder();

        public string Build() => sb.ToString();

        public HtmlBuilder Li(string innerHtml)
        {
            sb.Append("<li>");
            sb.Append(innerHtml);
            sb.Append("</li>");
            return this;
        }

        public HtmlBuilder TableStart(string id = null, string classList = null)
        {
            sb.Append("<table ");
            if (id != null)
            {
                sb.Append("id=");
                sb.Append(id);
            }
            if (classList != null)
            {
                sb.Append(" class=");
                sb.Append(classList); }

            return this;
        }

        public HtmlBuilder TableEnd() { sb.Append("</table>"); return this; }

        public HtmlBuilder TableHeader(IEnumerable<string> headerColumns)
        {
            sb.Append("<tr>");

            foreach(var col in headerColumns)
            {
                sb.Append("<th>");
                sb.Append(col);
                sb.Append("</th>");
            }

            sb.Append("</tr>");
            return this;
        }

        /// <summary>
        /// Format: List of rows.  Each row contains an array of columns.
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public HtmlBuilder TableData(List<string[]> rows, int colCount)
        {
            foreach (var row in rows)
            {
                sb.Append("<tr>");
                for (int i = 0; i < row.Length; i++)
                {
                    sb.Append("<td>");
                    sb.Append(row[i]);
                    sb.Append("</td>");
                }
                sb.Append("</tr>");
            }

            return this;
        }

        public HtmlBuilder TableRow(IEnumerable<string> columns)
        {
            sb.Append("<tr>");
            foreach(var c in columns)
            {
                sb.Append("<td>");
                sb.Append(c);
                sb.Append("</td>");
            }
            sb.Append("</tr>");
            return this;
        }

        public HtmlBuilder TableColumnsOnly(IEnumerable<string> columns)
        {
            foreach (var c in columns)
            {
                sb.Append("<td>");
                sb.Append(c);
                sb.Append("</td>");
            }
            return this;
        }
    }
}
