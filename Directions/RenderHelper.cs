using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.BaseExtention.Enums.Corporate;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Web.Settings;
using Kesco.Lib.Entities.Documents.EF.Directions;
using Kesco.Lib.Entities.Persons.PersonOld;
using System.Resources;
using System.Text;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities;
using Kesco.Lib.Log;

namespace Kesco.App.Web.Docs.Directions
{
    public class RenderHelper
    {
        protected ResourceManager LocalResx = new ResourceManager("Kesco.App.Web.Docs.Directions.DirectionIT",
          Assembly.GetExecutingAssembly());
        
        public void Photo(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.Value == null)
            {
                w.Write("&nbsp;");
                return;
            }

            w.Write(
                "<a href='#' onclick=\"window.open('{2}?id={1}','_blank','status=no,toolbar=no,menubar=no,location=no,resizable=yes,scrollbars=yes');\"><img style='padding-top:10px' src='{0}?id={1}&w=100' border=0 width='100px'></a>",
                Config.user_photo, dir.SotrudnikField.Value, Config.user_form);
        }
        
        public void Sotrudnik(EntityPage page, TextWriter w, Direction dir)
        {
            page.RenderLinkEmployee(w, "efSotrudnik", dir.Sotrudnik, NtfStatus.Empty);
        }

        public void SotrudnikPost(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikPostField.ValueString.Length > 0)
            {
                page.RenderNtf(w, new List<string> { dir.SotrudnikPostField.ValueString }, NtfStatus.Information);
                return;
            }

            var employee = dir.Sotrudnik;
            if (employee == null || employee.Unavailable)
            {
                w.Write("");
                return;
            }

            var dt = dir.SupervisorData;
            if (dt.Rows.Count != 1)
                dt = employee.GetUserPositions(CombineType.Неважно);

            if (dt.Rows.Count > 0)
            {
                dir.SotrudnikPostField.Value = dt.Rows[0]["ДолжностьСотрудника"];
                page.RenderNtf(w, new List<string> { dir.SotrudnikPostField.ValueString }, NtfStatus.Information);
            }
            else
                page.RenderNtf(w, new List<string> { "у сотрудника нет должности в штатном расписании" }, NtfStatus.Error);
        }

        public void Supervisor(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }

            var ws = new StringWriter();
            var sInfo = new List<string>();

            var sData = dir.SupervisorData;
            if (sData.Rows.Count == 0)
            {
                sInfo.Add("у сотрудника нет непосредственного руководителя");
                page.RenderNtf(w, sInfo, NtfStatus.Error);
                return;
            }


            ws.Write("Непосредственный руководитель - ");
            page.RenderLinkEmployee(ws, "btnSupervisor", sData.Rows[0]["КодРуководителя"].ToString(),
                sData.Rows[0]["Руководитель"].ToString(), NtfStatus.Information);

            ws.Write("<br/>");
            ws.Write(sData.Rows[0]["ДолжностьРуководителя"]);
            if (!sData.Rows[0]["КодЛицаКомпанииСотрудника"].Equals(sData.Rows[0]["КодЛицаКомпанииРуководителя"]))
            {
                ws.Write(" - ");
                page.RenderLinkPerson(ws, "btnSupevisorPerson", sData.Rows[0]["КодЛицаКомпанииРуководителя"].ToString(),
                    sData.Rows[0]["НазваниеКомпанииРуководителя"].ToString(), NtfStatus.Information);
            }

            sInfo.Add(ws.ToString());

            page.RenderNtf(w, sInfo, NtfStatus.Information);
        }

        private void PersonData(EntityPage page, TextWriter w, Direction dir, string personId, string defaultText, NtfStatus ntf)
        {
            PersonData(page, w, dir, personId, "", defaultText, ntf);
        }
        private void PersonData(EntityPage page, TextWriter w, Direction dir, string personId, string personName, string defaultText, NtfStatus ntf)
        {
            var name = "";
            if (personName.Length == 0)
            {
                var p = new PersonOld(personId);

                if (p.Unavailable)
                {
                    w.Write("{0} #{1}", defaultText, personId);
                    return;
                }

                name = p.GetName(dir.Date);
            }
            else name = personName;

            if (defaultText.Length > 0) w.Write(defaultText + " ");

            page.RenderLinkPerson(w, "", personId, name, ntf);
        }

        public void SotrudnikAOrg(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }

            var employee = dir.Sotrudnik;

            if (employee == null || employee.Unavailable)
            {
                w.Write("");
                return;
            }

            var wr = new StringWriter();
            var sInfo = new List<string>();

            var dt = dir.SupervisorData;
            if (dt.Rows.Count != 1)
                dt = employee.GetUserPositions(CombineType.ОсновноеМестоРаботы);

            if (dt.Rows.Count == 1 && employee.OrganizationId != null &&
                !employee.OrganizationId.Equals(dt.Rows[0]["КодЛицаКомпанииСотрудника"]))
            {
                dir.SotrudnikPostField.Value = dt.Rows[0]["ДолжностьСотрудника"];
                PersonData(page, wr, dir, dt.Rows[0]["КодЛицаКомпанииСотрудника"].ToString(), "", NtfStatus.Information);
                sInfo.Add(wr.ToString());
            }

            if (sInfo.Count > 0)
                page.RenderNtf(w, sInfo, NtfStatus.Information);
            else
                w.Write("");
        }
        public void SotrudnikFinOrg(EntityPage page, TextWriter w, Direction dir)
        {
            var employee = dir.Sotrudnik;
            if (employee == null || employee.Unavailable)
            {
                w.Write("");
                return;
            }

            var wr = new StringWriter();

            wr.Write(LocalResx.GetString("_lbl_ОтветственнаяКомпания") + " - ");

            if (employee.OrganizationId == null)
            {
                wr.Write("<font color='red'>");
                wr.Write(LocalResx.GetString("_Msg_СотрудникНетЛицаЗаказчика"));
                wr.Write("</font>");
                page.RenderNtf(w, new List<string>(new[] { wr.ToString() }), NtfStatus.Information);
                return;
            }

            PersonData(page, wr, dir, employee.OrganizationId.ToString(), "", NtfStatus.Information);
            page.RenderNtf(w, new List<string>(new[] { wr.ToString() }), NtfStatus.Information);
        }

        public void WorkPlaceType(EntityPage page, TextWriter w, Direction dir, bool renderDelete=true)
        {
            var bitmask = dir.WorkPlaceTypeField.ValueInt;
            StringBuilder sb = new StringBuilder();
            
            w.Write("<fieldset class=\"marginL paddingT paddingB paddingR marginT disp_inlineBlock\">");
            w.Write("<legend>{0}:</legend>", "Организовать сотруднику");
            if ((bitmask & 1) == 1)
            {
                w.Write("<div class=\"marginL disp_inlineBlockS\">{0}</div><br>", "рабочее место в офисе");
                if (dir.WorkPlaceField.ValueString.Length == 0)
                    page.RenderNtf(w, new List<string> {"не указано рабочее место"});
                else
                    WorkPlace(page, w, "", dir, renderDelete);

            }
            if ((bitmask & 2) == 2)
                w.Write("<div class=\"marginL marginT disp_inlineBlockS\">{0}</div><br>", "рабочее место вне офиса");

            if ((bitmask & 4) == 4)
                w.Write("<div class=\"marginL marginT disp_inlineBlockS\">{0}</div>", "доступ к корпоративной сети через Internet");

            w.Write("</fieldset>");
        }

        public void WorkPlace(EntityPage page, TextWriter w, string label, Direction dir, bool renderDelete=true)
        {
            if (dir.WorkPlaceField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }

            var ntf = new List<string>();

            w.Write(
                renderDelete
                    ? "<img src='/styles/delete.gif' border=0 onclick='directions_ClearWorkPlace();' style='cursor:pointer;' />"
                    : "<div class=\"marginL2\">");

            if (label.Length == 0 && dir.WorkPlaceField.ValueString.Length > 0)
            {
                var l = dir.LocationWorkPlace;
                if (l == null)
                {
                    w.Write("#");
                    w.Write(dir.WorkPlaceField.ValueString);
                    ntf.Add(LocalResx.GetString("_Msg_РабочееНеДоступно"));
                    page.RenderNtf(w, ntf);

                    return;
                }
                label = l.NamePath1;
            }

            page.RenderLinkLocation(w, dir.WorkPlaceField.ValueString, "");
            w.Write(label);
            page.RenderLinkEnd(w);

            if (!renderDelete)
            {
                w.Write("</div>");
                w.Write("<div class=\"marginL2\">");
            }
            
            ValidationMessages.CheckLocationWorkPlaceIsOffice(page, w, dir);
            ValidationMessages.CheckLocationWorkPlaceIsComputered(page, w, dir);
            ValidationMessages.CheckSotrudnikWorkPlace(page, w, dir);
            ValidationMessages.CheckEmployeesOnWorkPlace(page, w, dir);
            ValidationMessages.CheckExistsDirection(page, w, dir);

            w.Write(renderDelete ? "" : "</div>");

        }

        public string EquipmentEmployee(EntityPage page, TextWriter w, Direction dir, string idEq, string className)
        {
            var empls = "";
            var hasResp = false;
            var sqlParams = new Dictionary<string, object>
            {
                {"@id", int.Parse(idEq)},
            };

            using (
                var dbReader = new DBReader(SQLQueries.SELECT_ID_ОборудованиеСотрудников, CommandType.Text,
                    Config.DS_user, sqlParams))
            {
                if (dbReader.HasRows)
                {
                    hasResp = true;
                    var colКодСотрудника = dbReader.GetOrdinal("КодСотрудника");
                    var colСотрудник = dbReader.GetOrdinal("Сотрудник");

                    while (dbReader.Read())
                    {
                        using (var wr = new StringWriter())
                        {
                            var respId = dbReader.GetInt32(colКодСотрудника);
                            if (respId != dir.SotrudnikField.ValueInt)
                            {
                                page.RenderLinkEmployee(wr, "resp" + respId,
                                    dbReader.GetInt32(colКодСотрудника).ToString(), dbReader.GetString(colСотрудник),
                                    NtfStatus.Empty);
                                empls += (empls.Length > 0 ? ", " : "") + wr.ToString();
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(empls) && !hasResp)
                empls = " [" + LocalResx.GetString("_Msg_OpenEquipNotResp") + "]";
            else if (!string.IsNullOrEmpty(empls))
                empls = " [" + empls + "]";

            return empls;
        }
    }
}