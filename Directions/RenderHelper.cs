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
using Kesco.Lib.Entities.Corporate.Equipments;
using Kesco.Lib.Log;

namespace Kesco.App.Web.Docs.Directions
{
    public class RenderHelper
    {
       
        public void Photo(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.Value == null)
            {
                w.Write("&nbsp;");
                return;
            }

            w.Write(
                "<a href='#' onclick=\"window.open('{2}?id={1}','_blank','status=no,toolbar=no,menubar=no,location=no,resizable=yes,scrollbars=yes');\"><img style='padding-top:10px' src='{0}?id={1}&w=120' border=0 width='120px'></a>",
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
                page.RenderNtf(w, new List<string> { dir.SotrudnikPostField.ValueString }, NtfStatus.Empty, false, false);
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
                if (dir.SotrudnikPostField.Value.ToString()=="")
                    page.RenderNtf(w, new List<string> { "у сотрудника нет должности в штатном расписании" }, NtfStatus.Error);
                else 
                    page.RenderNtf(w, new List<string> { dir.SotrudnikPostField.ValueString }, NtfStatus.Empty, false, false);
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
                sData.Rows[0]["Руководитель"].ToString(), NtfStatus.Empty);

            ws.Write(" ");
            ws.Write(sData.Rows[0]["ДолжностьРуководителя"]);
            if (!sData.Rows[0]["КодЛицаКомпанииСотрудника"].Equals(sData.Rows[0]["КодЛицаКомпанииРуководителя"]))
            {
                ws.Write(" - ");
                page.RenderLinkPerson(ws, "btnSupevisorPerson", sData.Rows[0]["КодЛицаКомпанииРуководителя"].ToString(),
                    sData.Rows[0]["НазваниеКомпанииРуководителя"].ToString(), NtfStatus.Empty);
            }

            sInfo.Add(ws.ToString());

            page.RenderNtf(w, sInfo, NtfStatus.Empty, false, false);
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
                if (dt.Rows[0]["КодЛицаКомпанииСотрудника"].ToString().Length > 0)
                {
                    PersonData(page, wr, dir, dt.Rows[0]["КодЛицаКомпанииСотрудника"].ToString(), "",
                        NtfStatus.Empty);
                    sInfo.Add(wr.ToString());
                }
            }

            if (sInfo.Count > 0)
                page.RenderNtf(w, sInfo, NtfStatus.Empty,false, false);
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

            wr.Write(page.Resx.GetString("DIRECTIONS_lbl_ОтветственнаяКомпания") + " - ");

            if (employee.OrganizationId == null)
            {
                wr.Write("<font color='red'>");
                wr.Write(page.Resx.GetString("DIRECTIONS_Msg_СотрудникНетЛицаЗаказчика"));
                wr.Write("</font>");
                page.RenderNtf(w, new List<string>(new[] { wr.ToString() }), NtfStatus.Information);
                return;
            }

            PersonData(page, wr, dir, employee.OrganizationId.ToString(), "", NtfStatus.Empty);
            page.RenderNtf(w, new List<string>(new[] { wr.ToString() }), NtfStatus.Empty, false, false);
        }

        public void WorkPlaceType(EntityPage page, TextWriter w, Direction dir, bool renderDelete=true)
        {
            var bitmask = dir.WorkPlaceTypeField.ValueInt;
            if (bitmask == 0 && dir.WorkPlaceField.ValueString.Length > 0)
                bitmask = 1;

           
            w.Write("<fieldset class=\"marginL paddingT paddingB paddingR marginT disp_inlineBlock\">");
            w.Write("<legend>{0}:</legend>", page.Resx.GetString("DIRECTIONS_Field_WpType"));
            if ((bitmask & 1) == 1)
            {
                w.Write("<div class=\"marginL disp_inlineBlockS\">{0}</div><br>", page.Resx.GetString("DIRECTIONS_Field_WpType_1"));
                if (dir.WorkPlaceField.ValueString.Length == 0)
                    page.RenderNtf(w, new List<string> {"не указано рабочее место"});
                else
                    WorkPlace(page, w, "", dir, renderDelete);

            }
            if ((bitmask & 2) == 2)
                w.Write("<div class=\"marginL marginT disp_inlineBlockS\">{0}</div><br>", page.Resx.GetString("DIRECTIONS_Field_WpType_2"));

            if ((bitmask & 4) == 4)
                w.Write("<div class=\"marginL marginT disp_inlineBlockS\">{0}</div>", page.Resx.GetString("DIRECTIONS_Field_WpType_4"));

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
                    ntf.Add(page.Resx.GetString("DIRECTIONS_Msg_РабочееНеДоступно"));
                    page.RenderNtf(w, ntf);

                    return;
                }
                label = l.NamePath1;
                var icon = "";
                var title = "";

                dir.LocationWorkPlace.GetLocationSpecifications(out icon, out title);
                if (icon.Length > 0)
                    label+=string.Format("&nbsp;<img width=\"10\" height=\"10\" src=\"/styles/{0}\" title=\"{1}\" border=0/>", icon, title);


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

        public string EquipmentEmployee(EntityPage page, TextWriter w, Direction dir, string idEq, bool isNtf)
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
                            if (respId == dir.SotrudnikField.ValueInt || (!string.IsNullOrEmpty(dir.Sotrudnik.CommonEmployeeID) && respId == int.Parse(dir.Sotrudnik.CommonEmployeeID))) continue;

                            page.RenderLinkEmployee(wr, "resp" + respId,
                                dbReader.GetInt32(colКодСотрудника).ToString(), dbReader.GetString(colСотрудник),
                                NtfStatus.Error,
                                isNtf);
                            empls += (empls.Length > 0 ? ", " : "") + wr;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(empls) && !hasResp)
                empls = " [" + page.Resx.GetString("DIRECTIONS_Msg_OpenEquipNotResp") + "]";
            else if (!string.IsNullOrEmpty(empls))
                empls = " [" + empls + "]";

            return empls;
        }

        public void EquipmentInPlace(EntityPage page, string idContainer, Direction dir, string idLocation)
        {
            var w = new StringWriter();
            
            w.Write("<div style=\"font-weight:bold;\">");
            w.Write("Оборудование на расположении ");
            var loc = new Location(idLocation);
            if (!loc.Unavailable)
            {
                page.RenderLinkLocation(w, idLocation);
                w.Write(loc.NamePath1);
                page.RenderLinkEnd(w);
            }
            else
                w.Write(loc.Name);
            w.Write("</div>");
            
            var listEq = loc.EquipmentsIt;

            w.Write("<div class=\"marginT2\" style=\"overflow: auto; height: 130px\">");

            if (listEq.Count > 0)
            {
                listEq.OrderBy(x => x.TypeName).ToList().ForEach(delegate(Equipment eq)
                {
                    w.Write("<div style=\"white-space:nowrap;\">");
                    page.RenderLinkEquipment(w, eq.Id, "", "открыть форму оборудования");
                    w.Write("{0} {1}", eq.TypeName, eq.ModelName);
                    page.RenderLinkEnd(w);

                    if (eq.EmployeeId != dir.SotrudnikField.ValueInt)
                    {
                        w.Write(" [");

                        if (eq.EmployeeId != 0)
                            page.RenderLinkEmployee(w, "eml_" + eq.Id, eq.EmployeeId.ToString(), eq.EmployeeName,
                                NtfStatus.Empty);
                        else
                        {
                            w.Write("<span style=\"color:red\">{0}</span>", "не выдано");
                        }
                        w.Write("]");
                    }

                    w.Write("</div>");
                });
            }
            else
            {
                w.Write("<div>IT-оборудование отсутствует на данном расположении или у Вас нет прав на его просмотр.</div>");
            }

            w.Write("</div>");

            page.JS.Write("var obj_{0} = document.getElementById('{0}'); if(obj_{0}){{obj_{0}.innerHTML='{1}';}}", idContainer,
              HttpUtility.JavaScriptStringEncode(w.ToString()));
            page.JS.Write("directions_anotherEquipmentList();");
        }

        /// <summary>
        /// Оборудование сотрудника на чужих рабочих местах
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="idContainer">Контейнер вывода текста</param>
        /// <param name="dir">Текущий документ</param>
        public void EquipmentAnotherPlace(EntityPage page, string idContainer, Direction dir)
        {
            var w = new StringWriter();
            var listEq = dir.Sotrudnik.EmployeeEquipmentsAnotherWorkPlaces();
            w.Write("<div style=\"font-weight:bold;\">");
            w.Write("Оборудование сотрудника ");
            page.RenderLinkEmployee(w,"adinfEml",dir.SotrudnikField.ValueString, dir.Sotrudnik.FIO, NtfStatus.Empty);
            w.Write(", находится не на его рабочем месте");
            w.Write("</div>");
            w.Write("<div class=\"marginT2\" style=\"overflow: auto; height: 130px\">");
            if (listEq.Count > 0)
            {
                
                listEq.OrderBy(x => x.TypeName).ToList().ForEach(delegate(Equipment eq)
                {
                    w.Write("<div style=\"white-space:nowrap;\">");
                    page.RenderLinkEquipment(w, eq.Id,"","открыть форму оборудования");
                    w.Write("{0} {1}", eq.TypeName, eq.ModelName);
                    page.RenderLinkEnd(w);
                    w.Write(" -> ");
                    
                    if (eq.LocationId == 0) return;
                    page.RenderLinkLocation(w, eq.LocationId.ToString());
                    w.Write(eq.LocationName);
                    page.RenderLinkEnd(w);
                    w.Write("</div>");
                });
                
            }
            else
            {
                w.Write("нет данных об оборудовании на чужих рабочих местах");
            }
            w.Write("</div>");

            page.JS.Write("var obj_{0} = document.getElementById('{0}'); if(obj_{0}){{obj_{0}.innerHTML='{1}';}}", idContainer,
              HttpUtility.JavaScriptStringEncode(w.ToString()));
            page.JS.Write("directions_anotherEquipmentList();");
        }

        public void SotrudnikCadrWorkPlaces (EntityPage page, TextWriter w, Direction dir)
        {
            
            if (dir.SotrudnikField.ValueString.Length == 0)
                return;

            var wr = new StringWriter();
            var wps = dir.Sotrudnik.Workplaces;
            
            foreach (var wp in wps)
            {
                if (dir.WorkPlaceField.ValueString.Length > 0 && wp.Id == dir.WorkPlaceField.ValueString) continue;
                
                var icon = "";
                var title = "";

                wp.GetLocationSpecifications(out icon, out title);

                wr.Write("<div style=\"margin-left:30px;\">");
                page.RenderLinkLocation(wr, wp.Id, wp.Id, wp.Name, NtfStatus.Empty);
                if (icon.Length > 0)
                    wr.Write("&nbsp;<img width=\"10\" height=\"10\" src=\"/styles/{0}\" title=\"{1}\" border=0/>", icon, title);
                if ((dir.WorkPlaceField.ValueString.Length == 0 || dir.WorkPlaceField.ValueString.Length > 0 && wp.Id != dir.WorkPlaceField.ValueString) && wp.EquipmentsIt.Count>0)
                    wr.Write("&nbsp;<img width=\"10\" height=\"10\" src=\"/styles/detail.gif\" title=\"{0}\" style=\"cursor:pointer\" onclick=\"directions_openEquipment({1});\" border=0/>", page.Resx.GetString("DIRECTIONS_Title_ShowEq"), wp.Id);
                wr.Write("</div>");
            }
            var text = wr.ToString();
            if (text.Length > 0)
            {

                text = string.Format("{0}: {1}", page.Resx.GetString("DIRECTIONS_Lbl_WpsEpl"), text);
                page.RenderNtf(w, new List<string>(new[] { text }), NtfStatus.Empty, false, false);
            }

            var listEq = dir.Sotrudnik.EmployeeEquipmentsAnotherWorkPlaces();

            if (listEq.Count > 0)
            {
                wr = new StringWriter();
                wr.Write("{0} <img src=\"/styles/detail.gif\" width=\"10\" height=\"10\" style=\"cursor:pointer\" onclick=\"directions_openAnotherEquipment();\" title=\"{1}\"/>", page.Resx.GetString("DIRECTIONS_NTF_ExcessEq"), page.Resx.GetString("DIRECTIONS_Title_ShowEq"));
                page.RenderNtf(w, new List<string>(new[] {wr.ToString()}), NtfStatus.Error, false, false);
                
            }

        }

    }
}