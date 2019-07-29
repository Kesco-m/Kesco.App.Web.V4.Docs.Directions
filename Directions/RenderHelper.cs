using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.BaseExtention.Enums.Corporate;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities;
using Kesco.Lib.Entities.Corporate;
using Kesco.Lib.Entities.Corporate.Equipments;
using Kesco.Lib.Entities.Documents.EF.Directions;
using Kesco.Lib.Entities.Persons.PersonOld;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Web.Settings;

namespace Kesco.App.Web.Docs.Directions
{
    /// <summary>
    ///     Класс вывода дополнительной информации
    /// </summary>
    public class RenderHelper
    {
        /// <summary>
        ///     Вывод фотографии сотрудника
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
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

        /// <summary>
        ///     Вывод ссылки на сотрудника
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
        public void Sotrudnik(EntityPage page, TextWriter w, Direction dir)
        {
            page.RenderLinkEmployee(w, "efSotrudnik", dir.Sotrudnik, NtfStatus.Empty);
        }

        /// <summary>
        ///     Вывод информации о должности сотрудника
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
        /// <param name="validateSotrudik">Выполнять проверки сотрудника и рабочего места</param>
        public void SotrudnikPost(EntityPage page, TextWriter w, Direction dir, bool validateSotrudik = true)
        {
            var employee = dir.Sotrudnik;
            if (employee == null || employee.Unavailable)
            {
                w.Write("");
                return;
            }

            if (validateSotrudik)
            {
                ValidationMessages.CheckSotrudnik(page, w, dir, new List<Notification>());
                ValidationMessages.CheckSotrudnikWorkPlace(page, w, dir, new List<Notification>());
            }

            if (dir.SotrudnikPostField.ValueString.Length > 0)
                page.RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message = dir.SotrudnikPostField.ValueString,
                            Status = NtfStatus.Empty,
                            SizeIsNtf = false,
                            DashSpace = false
                        }
                    });

            var dt = dir.SupervisorData;
            if (dt.Rows.Count != 1)
                dt = employee.GetUserPositions(CombineType.Неважно);

            if (dt.Rows.Count > 0)
            {
                if (dir.SotrudnikPostField.Value.ToString() == "")
                {
                    if (dir.Sotrudnik.GroupMembers.Count == 0)
                        page.RenderNtf(w,
                            new List<Notification>
                            {
                                new Notification
                                {
                                    Message = page.Resx.GetString("DIRECTIONS_NTF_NoCadr"),
                                    Status = NtfStatus.Error,
                                    SizeIsNtf = false,
                                    DashSpace = false,
                                    Description = page.Resx.GetString("DIRECTIONS_NTF_NoCadr_Title")
                                }
                            });
                }
                else
                {
                    if (!dir.SotrudnikPostField.ValueString.Equals(dt.Rows[0]["ДолжностьСотрудника"]))
                        page.RenderNtf(w,
                            new List<Notification>
                            {
                                new Notification
                                {
                                    Message = page.Resx.GetString("DIRECTIONS_NTF_NoCadr"),
                                    Status = NtfStatus.Error,
                                    SizeIsNtf = false,
                                    DashSpace = false,
                                    Description = page.Resx.GetString("DIRECTIONS_NTF_NoCadr_Title")
                                }
                            });
                }
            }
            else
            {
                if (dir.Sotrudnik.GroupMembers.Count == 0)
                    page.RenderNtf(w,
                        new List<Notification>
                        {
                            new Notification
                            {
                                Message = page.Resx.GetString("DIRECTIONS_NTF_NoCadr"),
                                Status = NtfStatus.Error,
                                SizeIsNtf = false,
                                DashSpace = false,
                                Description = page.Resx.GetString("DIRECTIONS_NTF_NoCadr_Title")
                            }
                        });
            }
        }

        /// <summary>
        ///     Вывод списка рабочих мест сорудника по инвентаризации
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
        public void SotrudnikCadrWorkPlaces(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.ValueString.Length == 0)
                return;

            var wr = new StringWriter();
            var wps = dir.Sotrudnik.Workplaces;

            foreach (var wp in wps)
            {
                if (dir.WorkPlaceField.ValueString.Length > 0 && wp.Id == dir.WorkPlaceField.ValueString) continue;

                string icon;
                string title;

                wp.GetLocationSpecifications(out icon, out title);

                title = page.Resx.GetString(title);

                title = wp.IsOrganized
                    ? $"{title} - {page.Resx.GetString("DIRECTIONS_Msg_РаботаОрганизована")}"
                    : $"{title} - {page.Resx.GetString("DIRECTIONS_Msg_РаботаНеОрганизована")}";

                wr.Write("<div style=\"margin-left:30px;\">");
                page.RenderLinkLocation(wr, wp.Id, wp.Id, wp.Name);
                if (icon.Length > 0)
                    wr.Write("&nbsp;<img width=\"10\" height=\"10\" src=\"/styles/{0}\" title=\"{1}\" border=0/>", icon,
                        title);
                if ((dir.WorkPlaceField.ValueString.Length == 0 || dir.WorkPlaceField.ValueString.Length > 0 &&
                     wp.Id != dir.WorkPlaceField.ValueString) && wp.ExistEquipmentsIt)
                    wr.Write(
                        "&nbsp;<img width=\"10\" height=\"10\" src=\"/styles/detail.gif\" title=\"{0}\" style=\"cursor:pointer\" onclick=\"directions_openEquipment({1});\" border=0/>",
                        page.Resx.GetString("DIRECTIONS_Title_ShowEq"), wp.Id);
                wr.Write("</div>");
            }

            var text = wr.ToString();
            if (text.Length > 0)
            {
                text = $"{page.Resx.GetString("DIRECTIONS_Lbl_WpsEpl")}: {text}";
                page.RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message = text,
                            Status = NtfStatus.Empty,
                            SizeIsNtf = false,
                            DashSpace = false
                        }
                    });
            }

            //var listEq = dir.Sotrudnik.EmployeeEquipmentsAnotherWorkPlaces();

            //if (listEq.Count > 0)
            //{
            //    wr = new StringWriter();
            //    wr.Write(
            //        "{0} <img src=\"/styles/detail.gif\" width=\"10\" height=\"10\" style=\"cursor:pointer\" onclick=\"directions_openAnotherEquipment();\" title=\"{1}\"/>",
            //        page.Resx.GetString("DIRECTIONS_NTF_ExcessEq"), page.Resx.GetString("DIRECTIONS_Title_ShowEq"));
            //    page.RenderNtf(w,
            //        new List<Notification>
            //        {
            //            new Notification
            //            {
            //                Message = wr.ToString(),
            //                Status = NtfStatus.Error,
            //                SizeIsNtf = false,
            //                DashSpace = false,
            //                Description = page.Resx.GetString("DIRECTIONS_NTF_ExcessEq_Title")
            //            }
            //        });
            //}
        }

        /// <summary>
        ///     Вывод информации о непосредственном руководителе
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
        public void Supervisor(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.SotrudnikPostField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }

            var ws = new StringWriter();
            var sData = dir.SupervisorData;
            if (sData.Rows.Count == 0)
            {
                page.RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message = page.Resx.GetString("DIRECTIONS_NTF_NoLider1"),
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = page.Resx.GetString("DIRECTIONS_NTF_NoLider1_Title")
                        }
                    });
                return;
            }


            ws.Write($"{page.Resx.GetString("DIRECTIONS_Field_Lider")} - ");
            page.RenderLinkEmployee(ws, "btnSupervisor", sData.Rows[0]["КодРуководителя"].ToString(),
                page.IsRusLocal
                    ? sData.Rows[0]["Руководитель"].ToString()
                    : sData.Rows[0]["РуководительЛат"].ToString(), NtfStatus.Empty);

            ws.Write(" ");
            ws.Write(sData.Rows[0]["ДолжностьРуководителя"]);
            if (!sData.Rows[0]["КодЛицаКомпанииСотрудника"].Equals(sData.Rows[0]["КодЛицаКомпанииРуководителя"]))
            {
                ws.Write(" - ");
                page.RenderLinkPerson(ws, "btnSupevisorPerson", sData.Rows[0]["КодЛицаКомпанииРуководителя"].ToString(),
                    sData.Rows[0]["НазваниеКомпанииРуководителя"].ToString());
            }

            page.RenderNtf(w,
                new List<Notification>
                {
                    new Notification
                    {
                        Message = ws.ToString(),
                        Status = NtfStatus.Empty,
                        SizeIsNtf = false,
                        DashSpace = false
                    }
                });
        }

        /// <summary>
        ///     Вывод информации по лицу
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
        /// <param name="personId">Идентификатор лица</param>
        /// <param name="defaultText">Текст по-умолчанию</param>
        /// <param name="ntf">Класс нотификации</param>
        private void PersonData(EntityPage page, TextWriter w, Direction dir, string personId, string defaultText,
            NtfStatus ntf)
        {
            PersonData(page, w, dir, personId, "", defaultText, ntf);
        }

        /// <summary>
        ///     Вывод информации по лицу
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
        /// <param name="personId">Идентификатор лица</param>
        /// <param name="personName">Название лица</param>
        /// <param name="defaultText">Текст по-умолчанию</param>
        /// <param name="ntf">>Класс нотификации</param>
        private void PersonData(EntityPage page, TextWriter w, Direction dir, string personId, string personName,
            string defaultText, NtfStatus ntf)
        {
            string name;
            if (personName.Length == 0)
            {
                var personObject = page.GetObjectById(typeof(PersonOld), personId) as PersonOld;
                if (personObject == null || personObject.Unavailable)
                {
                    w.Write("{0} #{1}", defaultText, personId);
                    return;
                }

                name = personObject.GetName(dir.Date);
            }
            else
            {
                name = personName;
            }

            if (defaultText.Length > 0) w.Write(defaultText + " ");

            page.RenderLinkPerson(w, "", personId, name, ntf);
        }

        /// <summary>
        ///     Вывод информации о работодателе сотрудника
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
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
            var sInfo = new List<Notification>();

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
                    sInfo.Add(new Notification
                    {
                        Message = wr.ToString(),
                        Status = NtfStatus.Empty,
                        SizeIsNtf = false,
                        DashSpace = false
                    });
                }
            }

            if (sInfo.Count > 0)
                page.RenderNtf(w, sInfo);
            else
                w.Write("");
        }

        /// <summary>
        ///     Вывод информации о лице заказчике
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
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
                page.RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message = wr.ToString(),
                            Status = NtfStatus.Information,
                            SizeIsNtf = false,
                            DashSpace = false
                        }
                    });
                return;
            }

            PersonData(page, wr, dir, employee.OrganizationId.ToString(), "", NtfStatus.Empty);
            page.RenderNtf(w,
                new List<Notification>
                {
                    new Notification
                    {
                        Message = wr.ToString(),
                        Status = NtfStatus.Empty,
                        SizeIsNtf = false,
                        DashSpace = false
                    }
                });
        }

        /// <summary>
        ///     Вывод информации в зависимости от того, что требуется организовать
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
        /// <param name="renderDelete">Выводить ли кнопку удаления выбранного рабочего места</param>
        public void WorkPlaceType(EntityPage page, TextWriter w, Direction dir, bool renderDelete = true)
        {
            var bitmask = dir.WorkPlaceTypeField.ValueInt;
            if (bitmask == 0 && dir.WorkPlaceField.ValueString.Length > 0)
                bitmask = 1;


            w.Write("<fieldset class=\"marginL paddingT paddingB paddingR marginT disp_inlineBlock\">");
            w.Write("<legend>{0}:</legend>", page.Resx.GetString("DIRECTIONS_Field_WpType"));
            if ((bitmask & 1) == 1)
            {
                w.Write("<div class=\"marginL disp_inlineBlockS\">{0}</div><br>",
                    page.Resx.GetString("DIRECTIONS_Field_WpType_1"));
                if (dir.WorkPlaceField.ValueString.Length == 0)
                    page.RenderNtf(w,
                        new List<Notification>
                        {
                            new Notification
                            {
                                Message = page.Resx.GetString("DIRECTIONS_NTF_NoWorkPlace"),
                                Status = NtfStatus.Error,
                                SizeIsNtf = false,
                                DashSpace = false,
                                Description = page.Resx.GetString("DIRECTIONS_NTF_NoWorkPlace_Title")
                            }
                        });

                else
                    WorkPlace(page, w, "", dir, renderDelete);
            }

            if ((bitmask & 2) == 2)
            {
                w.Write("<div class=\"marginL marginT disp_inlineBlockS\">{0}",
                    page.Resx.GetString("DIRECTIONS_Field_WpType_2"));

                ValidationMessages.CheckExistsWorkPlaceIsComputeredNoOrganized(page, w, dir);

                w.Write("</div><br>");
            }

            if ((bitmask & 4) == 4)
                w.Write("<div class=\"marginL marginT disp_inlineBlockS\">{0}</div>",
                    page.Resx.GetString("DIRECTIONS_Field_WpType_4"));

            w.Write("</fieldset>");
        }

        /// <summary>
        ///     Вывод информации в зависимости от того, что требуется организовать
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="label">Напись</param>
        /// <param name="dir">Объект документа</param>
        /// <param name="renderDelete">Выводить ли кнопку удаления выбранного рабочего места</param>
        public void WorkPlace(EntityPage page, TextWriter w, string label, Direction dir, bool renderDelete = true)
        {
            if (dir.WorkPlaceField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }

            var ntf = new List<Notification>();

            w.Write(
                renderDelete
                    ? "<img src='/styles/delete.gif' border=0 onclick='directions_ClearWorkPlace();' style='cursor:pointer;' />"
                    : "<div class=\"marginL2\">");
            var iconString = "";
            if (label.Length == 0 && dir.WorkPlaceField.ValueString.Length > 0)
            {
                var l = page.GetObjectById(typeof(Location), dir.WorkPlaceField.ValueString) as Location;
                if (l == null || l.Unavailable)
                {
                    w.Write("#");
                    w.Write(dir.WorkPlaceField.ValueString);
                    ntf.Add(new Notification
                    {
                        Message = page.Resx.GetString("DIRECTIONS_Msg_РабочееНеДоступно"),
                        Status = NtfStatus.Error,
                        SizeIsNtf = false,
                        DashSpace = false,
                        Description = page.Resx.GetString("DIRECTIONS_Msg_РабочееНеДоступно_Title")
                    });
                    page.RenderNtf(w, ntf);


                    return;
                }

                label = l.NamePath1;
                string icon;
                string title;

                l.GetLocationSpecifications(out icon, out title);

                title = page.Resx.GetString(title);
                title = l.IsOrganized
                    ? $"{title} - {page.Resx.GetString("DIRECTIONS_Msg_РаботаОрганизована")}"
                    : $"{title} - {page.Resx.GetString("DIRECTIONS_Msg_РаботаНеОрганизована")}";

                if (icon.Length > 0)
                    iconString =
                        $"&nbsp;<img width=\"10\" height=\"10\" src=\"/styles/{icon}\" title=\"{title}\" border=0/>";
            }

            page.RenderLinkLocation(w, dir.WorkPlaceField.ValueString);
            w.Write(label);
            page.RenderLinkEnd(w);
            if (iconString.Length > 0)
                w.Write(iconString);
            if (!renderDelete)
            {
                w.Write("</div>");
                w.Write("<div class=\"marginL2\">");
            }

            ValidationMessages.CheckLocationWorkPlaceIsOffice(page, w, dir);
            ValidationMessages.CheckLocationWorkPlaceIsComputered(page, w, dir);
            ValidationMessages.CheckExistsWorkPlaceIsComputeredNoOrganized(page, w, dir, dir.WorkPlaceField.ValueInt);
            ValidationMessages.CheckSotrudnikWorkPlace(page, w, dir);
            ValidationMessages.CheckEmployeesOnWorkPlace(page, w, dir);
            ValidationMessages.CheckExistsDirection(page, w, dir);


            w.Write(renderDelete ? "" : "</div>");
        }

        /// <summary>
        ///     Вывод информации об оборудовании сотрудника
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
        /// <param name="idEq">Идентификатор оборудования</param>
        /// <param name="isNtf">Является ли выводимая информация нотификацией</param>
        /// <returns>список оборудования с ответственными в виде строки</returns>
        public string EquipmentEmployee(EntityPage page, TextWriter w, Direction dir, string idEq, bool isNtf)
        {
            var empls = "";
            var hasResp = false;
            var sqlParams = new Dictionary<string, object>
            {
                {"@id", int.Parse(idEq)}
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
                        using (var wr = new StringWriter())
                        {
                            var respId = dbReader.GetInt32(colКодСотрудника);
                            if (respId == dir.SotrudnikField.ValueInt ||
                                !string.IsNullOrEmpty(dir.Sotrudnik.CommonEmployeeID) &&
                                respId == int.Parse(dir.Sotrudnik.CommonEmployeeID)) continue;

                            page.RenderLinkEmployee(wr, "resp" + respId,
                                dbReader.GetInt32(colКодСотрудника).ToString(), dbReader.GetString(colСотрудник),
                                NtfStatus.Error,
                                isNtf);
                            empls += (empls.Length > 0 ? ", " : "") + wr;
                        }
                }
            }

            if (string.IsNullOrEmpty(empls) && !hasResp)
                empls = " [" + page.Resx.GetString("DIRECTIONS_Msg_OpenEquipNotResp") + "]";
            else if (!string.IsNullOrEmpty(empls))
                empls = " [" + empls + "]";

            return empls;
        }

        /// <summary>
        ///     Вывод информации об оборудовании на расположении(рабочем месте)
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="idContainer">Идентификтор html-контейнера, куда выводить информацию</param>
        /// <param name="dir">Объект документа</param>
        /// <param name="idLocation">Идентификатор расположения</param>
        public void EquipmentInPlace(EntityPage page, string idContainer, Direction dir, string idLocation)
        {
            var w = new StringWriter();

            w.Write("<div style=\"font-weight:bold;\">");
            w.Write($"{page.Resx.GetString("DIRECTIONS_Title_EquipmentInLocation")} ");

            var l = page.GetObjectById(typeof(Location), idLocation) as Location;
            if (l != null && !l.Unavailable)
            {
                page.RenderLinkLocation(w, idLocation);
                w.Write(l.NamePath1);
                page.RenderLinkEnd(w);
            }
            else
            {
                w.Write(l.Name);
            }

            w.Write("</div>");

            var listEq = l.EquipmentsIt;

            w.Write("<div class=\"marginT2\" style=\"overflow: auto; height: 130px\">");

            if (listEq.Count > 0)
                listEq.OrderBy(x => x.TypeName).ToList().ForEach(delegate(Equipment eq)
                {
                    w.Write("<div style=\"white-space:nowrap;\">");
                    page.RenderLinkEquipment(w, eq.Id, "", page.Resx.GetString("DIRECTIONS_Title_ShowEq1"));
                    w.Write("{0} {1}", eq.TypeName, eq.ModelName);
                    page.RenderLinkEnd(w);

                    if (eq.EmployeeId != dir.SotrudnikField.ValueInt)
                    {
                        w.Write(" [");

                        if (eq.EmployeeId != 0)
                            page.RenderLinkEmployee(w, "eml_" + eq.Id, eq.EmployeeId.ToString(), eq.EmployeeName,
                                NtfStatus.Empty);
                        else
                            w.Write("<span style=\"color:red\">{0}</span>",
                                page.Resx.GetString("DIRECTIONS_NTF_NoIssued"));
                        w.Write("]");
                    }

                    w.Write("</div>");
                });
            else
                w.Write($"<div>{page.Resx.GetString("DIRECTIONS_NTF_NoEquipment")}.</div>");

            w.Write("</div>");

            page.JS.Write("var obj_{0} = document.getElementById('{0}'); if(obj_{0}){{obj_{0}.innerHTML='{1}';}}",
                idContainer,
                HttpUtility.JavaScriptStringEncode(w.ToString()));
            page.JS.Write("directions_anotherEquipmentList();");
        }


        /// <summary>
        ///     Оборудование сотрудника на чужих рабочих местах
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="idContainer">Идентификтор html-контейнера, куда выводить информацию</param>
        /// <param name="dir">Текущий документ</param>
        public void EquipmentAnotherPlace(EntityPage page, string idContainer, Direction dir)
        {
            var w = new StringWriter();
            var listEq = dir.Sotrudnik.EmployeeEquipmentsAnotherWorkPlaces();
            w.Write("<div style=\"font-weight:bold;\">");
            w.Write($"{page.Resx.GetString("DIRECTIONS_Title_EquipmentNoLocation1")} ");
            page.RenderLinkEmployee(w, "adinfEml", dir.SotrudnikField.ValueString, dir.Sotrudnik.FIO, NtfStatus.Empty);
            w.Write(page.Resx.GetString("DIRECTIONS_Title_EquipmentNoLocation2"));
            w.Write("</div>");
            w.Write("<div class=\"marginT2\" style=\"overflow: auto; height: 130px\">");
            if (listEq.Count > 0)
                listEq.OrderBy(x => x.TypeName).ToList().ForEach(delegate(Equipment eq)
                {
                    w.Write("<div style=\"white-space:nowrap;\">");
                    page.RenderLinkEquipment(w, eq.Id, "", page.Resx.GetString("DIRECTIONS_Title_ShowEq1"));
                    w.Write("{0} {1}", eq.TypeName, eq.ModelName);
                    page.RenderLinkEnd(w);
                    w.Write(" -> ");

                    if (eq.LocationId == 0) return;
                    page.RenderLinkLocation(w, eq.LocationId.ToString());
                    w.Write(eq.LocationName);
                    page.RenderLinkEnd(w);
                    w.Write("</div>");
                });
            else
                w.Write(page.Resx.GetString("DIRECTIONS_Title_EquipmentNoLocatio3"));
            w.Write("</div>");

            page.JS.Write("var obj_{0} = document.getElementById('{0}'); if(obj_{0}){{obj_{0}.innerHTML='{1}';}}",
                idContainer,
                HttpUtility.JavaScriptStringEncode(w.ToString()));
            page.JS.Write("directions_anotherEquipmentList();");
        }

        /// <summary>
        ///     Вывод проверок значения в поле e-mail
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
        public void EmailCheck(EntityPage page, TextWriter w, Direction dir)
        {
            var ntfs = new List<Notification>();

            using (var wr = new StringWriter())
            {
                ValidationMessages.CheckEmailName(page, wr, dir);
                var ws = wr.ToString();
                if (ws.Length > 0)
                    ntfs.Add(
                        new Notification
                        {
                            Message = ws,
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = page.Resx.GetString("DIRECTIONS_Msg_EmailНеКорректен_Title")
                        }
                    );
            }

            using (var wr = new StringWriter())
            {
                ValidationMessages.CheckUniqueEmail(page, wr, dir);
                var ws = wr.ToString();
                if (ws.Length > 0)
                    ntfs.Add(
                        new Notification
                        {
                            Message = ws,
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = page.Resx.GetString("DIRECTIONS_NTF_EmailNotUnique_Title")
                        }
                    );
            }

            using (var wr = new StringWriter())
            {
                ValidationMessages.CheckEmailNameSortudnik(page, wr, dir);
                var ws = wr.ToString();
                if (ws.Length > 0)
                    ntfs.Add(
                        new Notification
                        {
                            Message = ws,
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = page.Resx.GetString("DIRECTIONS_NTF_EmailName_Title")
                        }
                    );
            }

            using (var wr = new StringWriter())
            {
                ValidationMessages.CheckEmailDomainByPerson(page, wr, dir);
                var ws = wr.ToString();
                if (ws.Length > 0)
                    ntfs.Add(
                        new Notification
                        {
                            Message = ws,
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = page.Resx.GetString("DIRECTIONS_NTF_EmailDomain_Title")
                        }
                    );
            }

            if (ntfs.Count > 0)
                page.RenderNtf(w, ntfs);
        }

        /// <summary>
        ///     Вывод проверок значения в поле логин
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
        /// <param name="ntfs">Список с сообщениями</param>
        /// <param name="render">Выводить ли сообщения в поток</param>
        public void LoginCheck(EntityPage page, TextWriter w, Direction dir, List<Notification> ntfs, bool render)
        {
            string ws;
            using (var wr = new StringWriter())
            {
                ValidationMessages.CheckLogin(page, wr, dir);
                ws = wr.ToString();
                if (ws.Length > 0)
                    ntfs.Add(new Notification
                    {
                        Message = ws,
                        Status = NtfStatus.Error,
                        SizeIsNtf = false,
                        DashSpace = false,
                        Description = page.Resx.GetString("DIRECTIONS_Msg_ЛогинНеКорректен_Title")
                    });
            }

            using (var wr = new StringWriter())
            {
                ValidationMessages.CheckLoginName(page, wr, dir);
                ws = wr.ToString();
                if (ws.Length > 0)
                    ntfs.Add(new Notification
                    {
                        Message = ws,
                        Status = NtfStatus.Error,
                        SizeIsNtf = false,
                        DashSpace = false,
                        Description = page.Resx.GetString("DIRECTIONS_Msg_ЛогинИмя_Title")
                    });
            }

            using (var wr = new StringWriter())
            {
                ValidationMessages.CheckUniqueLogin(page, wr, dir);
                ws = wr.ToString();
                if (ws.Length > 0)
                    ntfs.Add(new Notification
                    {
                        Message = ws,
                        Status = NtfStatus.Error,
                        SizeIsNtf = false,
                        DashSpace = false,
                        Description = page.Resx.GetString("DIRECTIONS_Msg_ЛогинСуществует_Title")
                    });
            }

            if (ntfs.Count > 0 && render)
                page.RenderNtf(w, ntfs);
        }

        /// <summary>
        ///     Вывод информации о группе(название и ссылка) сотрудника
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
        public void CommonEmployee(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }

            var employee = dir.Sotrudnik;
            if (employee == null || employee.Unavailable || string.IsNullOrEmpty(employee.CommonEmployeeID))
            {
                w.Write("");
                return;
            }

            var commonEmployee = new Employee(employee.CommonEmployeeID);
            w.Write($"{page.Resx.GetString("DIRECTIONS_NTF_ГруппаПосменнойРаботы")}: ");
            page.RenderLinkEmployee(w, "linkCommonEmployee", commonEmployee, NtfStatus.Empty, false);
            w.Write(
                "&nbsp;<img width=\"10\" height=\"10\" src=\"/styles/detail.gif\" title=\"{0}\" style=\"cursor:pointer\" onclick=\"directions_groupMembers({1});\" border=0/>",
                page.Resx.GetString("DIRECTIONS_Title_GroupMembers"), commonEmployee.Id);
        }

        /// <summary>
        ///     Вывод информации о составе группы посменной работы
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="idContainer">Идентификтор html-контейнера, куда выводить информацию</param>
        /// <param name="dir">Объект документа</param>
        /// <param name="idGroup">Идентификатор группы</param>
        public void GroupMembers(EntityPage page, string idContainer, Direction dir, string idGroup)
        {
            var w = new StringWriter();

            w.Write("<div style=\"font-weight:bold;\">");
            w.Write($"{page.Resx.GetString("DIRECTIONS_Title_GroupMembers1")} ");
            var employee = new Employee(idGroup);
            if (!employee.Unavailable)
                page.RenderLinkEmployee(w, "linkGroupMembers", employee, NtfStatus.Empty, false);
            else
                w.Write($"#{idGroup}");
            w.Write(":</div>");

            var listEq = employee.GroupMembers;

            w.Write("<div class=\"marginT2\" style=\"overflow: auto; height: 130px\">");

            if (listEq.Count > 0)
                listEq.OrderBy(x => x.FIO).ToList().ForEach(delegate(Employee gm)
                {
                    w.Write("<div style=\"white-space:nowrap;\">");
                    page.RenderLinkEmployee(w, $"linkGroupMembers{gm.Id}", gm, NtfStatus.Empty, false);
                    w.Write("</div>");
                });
            else
                w.Write($"<div>{page.Resx.GetString("DIRECTIONS_NTF_EmptyGroup")}.</div>");

            w.Write("</div>");

            page.JS.Write("var obj_{0} = document.getElementById('{0}'); if(obj_{0}){{obj_{0}.innerHTML='{1}';}}",
                idContainer,
                HttpUtility.JavaScriptStringEncode(w.ToString()));
            page.JS.Write("directions_anotherEquipmentList();");
        }

        public void AdvancedGrants(EntityPage page, TextWriter w, Direction dir, int bitMask)
        {
            var ags = dir.AdvancedGrantsAvailable.FindAll(x => x.RefersTo == bitMask);
            var inx = 0;
            ags.OrderBy(x => x.OrderOutput).ToList().ForEach(delegate(AdvancedGrant grant)
            {
                var agChecked = dir.PositionAdvancedGrants.Any(x => x.GrantId.ToString() == grant.Id);
                var agDisabled = grant.NotAlive || bitMask == 1 && dir.SotrudnikField.ValueString.Length > 0 &&
                                 !dir.Sotrudnik.Unavailable && dir.Sotrudnik.Login.Length > 0;
                var position = dir.PositionAdvancedGrants.FirstOrDefault(x => x.GrantId.ToString() == grant.Id);
                var guidId = position?.GuidId ?? Guid.NewGuid();
                var positionId = position?.Id ?? "0";


                w.Write("<div class=\"{0}\">", inx > 0 ? "marginT2" : "");
                w.Write("<div class=\"disp_inlineBlockS\">");
                w.Write(
                    "<input type=\"checkbox\" id=\"efAgvGrant{0}\" {1} {2} onclick=\"cmdasync('cmd', 'EditPositionAG', 'PositionId', {3}, 'GuidId', '{4}', 'GrantId', {5}, 'WhatDo', this.checked?1:0);\"/>",
                    grant.Id,
                    agChecked ? "checked" : "",
                    agDisabled ? "disabled" : "",
                    positionId,
                    guidId,
                    grant.Id);
                w.Write("</div>");
                w.Write("<div class=\"disp_inlineBlockS marginL\">");
                w.Write($"<label for=\"efAgvGrant{grant.Id}\">");
                w.Write(page.IsRusLocal ? grant.Name : grant.NameEn);
                w.Write("</label>");
                w.Write("</div>");
                w.Write("</div>");
                inx++;
            });
        }



        #region ADSI

        /// <summary>
        ///     Получение и вывод информации о пути в AD по логину
        /// </summary>
        /// <param name="w">Поток вывода</param>
        /// <param name="ntfs">Словарь данных</param>
        /// <param name="login">Логин</param>
        /// <param name="bitmask">Битовая маска</param>
        /// <returns>Путь в AD</returns>
        public string ADSI_RenderInfoByLogin(EntityPage page, TextWriter w, List<Notification> ntfs, string login, Employee empl,
            int bitmask)
        {
            var sqlParams = new Dictionary<string, object>
            {
                {"@Login", login}
            };
            var adsiPath = "";
            using (
                var dbReader = new DBReader(SQLQueries.SELECT_ADSI_ПоЛогину, CommandType.Text, Config.DS_user,
                    sqlParams)
            )
            {
                if (!dbReader.HasRows)
                {
                    w.Write("");
                    return adsiPath;
                }

                var colPath = dbReader.GetOrdinal("Path");
                var colDisabled = dbReader.GetOrdinal("Disabled");
                var colAccountExpires = dbReader.GetOrdinal("accountExpires");

                while (dbReader.Read())
                {
                    ADSI_RenderPath(page, dbReader, colPath, w, out adsiPath, false, false);
                    ADSI_RenderAccountExpires(page, ntfs, dbReader, empl, colAccountExpires, bitmask);
                    ADSI_RenderDisabled(page, ntfs, dbReader, empl, colDisabled, bitmask);
                    return adsiPath;
                }
            }

            return adsiPath;
        }

        /// <summary>
        ///     Получение и вывод информации о пути в AD по коду сотрудника
        /// </summary>
        /// <param name="w">Поток вывода</param>
        /// <param name="ntfs">Словарь данных</param>
        /// <param name="employeeId">Код сотрудника</param>
        /// <param name="bitmask">Битовая маска</param>
        /// <returns>Путь в AD</returns>
        public string ADSI_RenderInfoByEmployee(EntityPage page, TextWriter w, List<Notification> ntfs, Employee empl, int bitmask)
        {
            var sqlParams = new Dictionary<string, object>
            {
                {"@КодСотрудника", empl.Id}
            };
            var adsiPath = "";
            using (
                var dbReader = new DBReader(SQLQueries.SELECT_ADSI_ПоКодуСотрудника, CommandType.Text, Config.DS_user,
                    sqlParams)
            )
            {
                if (!dbReader.HasRows)
                {
                    w.Write("");
                    return adsiPath;
                }

                var colPath = dbReader.GetOrdinal("Path");
                var colDisabled = dbReader.GetOrdinal("Disabled");
                var colAccountExpires = dbReader.GetOrdinal("accountExpires");

                while (dbReader.Read())
                {
                    ADSI_RenderPath(page, dbReader, colPath, w, out adsiPath, false, false);
                    ADSI_RenderAccountExpires(page, ntfs, dbReader, empl, colAccountExpires, bitmask);
                    ADSI_RenderDisabled(page, ntfs, dbReader, empl, colDisabled, bitmask);
                    return adsiPath;
                }
            }

            return adsiPath;
        }

        /// <summary>
        ///     Получение информации об отключении учетной записи из AD
        /// </summary>
        /// <param name="ntfs">Словарь данных</param>
        /// <param name="dbReader">Открытый источник данных</param>
        /// <param name="empl">Сотрудник</param>
        /// <param name="colDisabled">Индекс колонки</param>
        /// <param name="bitMask">Битовая маска</param>
        private void ADSI_RenderDisabled(EntityPage page, List<Notification> ntfs, IDataRecord dbReader, Employee empl, int colDisabled, int bitMask)
        {
            if (dbReader.GetInt32(colDisabled) == 0) return;

            ntfs.Add(new Notification
            {
                Message = "Disabled",
                Status = ((bitMask & 2) == 2 && empl.Status == 3) ? NtfStatus.Empty : NtfStatus.Error,
                SizeIsNtf = false,
                DashSpace = false,
                Description = page.Resx.GetString("DIRECTIONS_NTF_Disabled_Title")
            });
        }

        /// <summary>
        ///     Получение информации о сроке действия учетной записи AD
        /// </summary>
        /// <param name="ntfs">Словарь данных</param>
        /// <param name="dbReader">Открытый источник данных</param>
        /// <param name="empl">Сотрудник</param>
        /// <param name="colAccountExpires">Индекс колонки</param>
        /// <param name="bitMask">Битовая маска</param>
        private void ADSI_RenderAccountExpires(EntityPage page, List<Notification> ntfs, DBReader dbReader, Employee empl, int colAccountExpires, int bitMask)
        {
            var accountExpiresIsDbNull = dbReader.IsDBNull(colAccountExpires);
            if (accountExpiresIsDbNull && (bitMask & 2) == 2 && empl.Status == 3)
                ntfs.Add(new Notification
                {
                    Message = page.Resx.GetString("DIRECTIONS_NTF_AccountExpiresNo"),
                    Status = NtfStatus.Error,
                    SizeIsNtf = false,
                    DashSpace = false,
                    Description = page.Resx.GetString("DIRECTIONS_NTF_AccountExpiresNo_Title")
                });
            else if (!accountExpiresIsDbNull && (bitMask & 1) == 1)
                ntfs.Add(new Notification
                {
                    Message = "AccountExpires: " + dbReader.GetDateTime(colAccountExpires).ToString("dd.MM.yy"),
                    Status = NtfStatus.Error,
                    SizeIsNtf = false,
                    DashSpace = false,
                    Description = page.Resx.GetString("DIRECTIONS_NTF_AccountExpires_Title")
                });
            else if (!accountExpiresIsDbNull && (bitMask & 2) == 2)
                ntfs.Add(new Notification
                {
                    Message = "AccountExpires: " + dbReader.GetDateTime(colAccountExpires).ToString("dd.MM.yy"),
                    Status = ((bitMask & 2) == 2 && empl.Status == 3) ? NtfStatus.Empty : NtfStatus.Error,
                    SizeIsNtf = false,
                    DashSpace = false
                });
        }

        /// <summary>
        ///     Вывод информации о пути в AD
        /// </summary>
        /// <param name="dbReader">Открытый источник данных</param>
        /// <param name="colPath">Индекс колонки</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="adsiPath">Путь в AD</param>
        /// <param name="marginL">Нужен ли отступ слева</param>
        /// <param name="streamWrite">Выводить в поток</param>
        private void ADSI_RenderPath(EntityPage page, DBReader dbReader, int colPath, TextWriter w, out string adsiPath, bool marginL = true, bool streamWrite = true)
        {
            var path = dbReader.GetString(colPath);

            var regex = new Regex("(OU=.+?(,|$))", RegexOptions.IgnoreCase);
            var matches = regex.Matches(path);
            if (streamWrite) w.Write("<div class=\"{0}\">", marginL ? "marginL" : "");
            adsiPath = matches.Cast<object>().Aggregate("", (current, m) => current + m);

            if (adsiPath.Right(1) == ",")
                adsiPath = adsiPath.Left(adsiPath.Length - 1);

            if (!streamWrite) return;

            w.Write(adsiPath);
            w.Write("</div>");
        }

        #endregion
    }
}