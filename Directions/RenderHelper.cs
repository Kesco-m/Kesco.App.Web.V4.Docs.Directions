using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.BaseExtention.Enums.Corporate;
using Kesco.Lib.BaseExtention.Enums.Docs;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities;
using Kesco.Lib.Entities.Corporate;
using Kesco.Lib.Entities.Corporate.Equipments;
using Kesco.Lib.Entities.Documents.EF.Directions;
using Kesco.Lib.Entities.Persons.PersonOld;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Web.Controls.V4.Common.DocumentPage;
using Kesco.Lib.Web.Settings;
using Role = Kesco.Lib.BaseExtention.Enums.Corporate.Role;

namespace Kesco.App.Web.Docs.Directions
{
    /// <summary>
    ///     Класс вывода дополнительной информации
    /// </summary>
    public class RenderHelper
    {
        private readonly List<int> SotrudnikCompanies = new List<int>();


        /// <summary>
        ///     Вывод фотографии сотрудника
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
        public void Photo(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir == null) return;
            if (dir.SotrudnikField.Value == null)
            {
                w.Write("&nbsp;");
                return;
            }

            w.Write(
                "<a href='#' onclick=\"Kesco.windowOpen('{2}?id={1}');\"><img src='{0}?id={1}&w=120' border=0 width='120px'></a>",
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
            if (dir == null) return;
            page.RenderLinkEmployee(w, "efSotrudnik", dir.Sotrudnik, NtfStatus.Empty);

            var ntfList = new List<Notification>();
            ValidationMessages.CheckSotrudnikStatus(page, w, dir, ntfList);
            if (ntfList.Count > 0)
                page.RenderNtf(w, ntfList);
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
            if (dir == null) return;
            var employee = dir.Sotrudnik;
            if (employee == null || employee.Unavailable)
            {
                w.Write("");
                return;
            }

            if (validateSotrudik && !dir.Finished)
            {
                ValidationMessages.CheckSotrudnik(page, w, dir, new List<Notification>());               
            }

            var dt = employee.GetUserPositions(CombineType.Неважно);
            if (dt.Rows.Count == 0 && !employee.IsGroupWork)
            {
                page.RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message = dir.Resx.GetString("DIRECTIONS_NTF_NoCadr"),
                            Status = NtfStatus.Error,
                            Description = dir.Resx.GetString("DIRECTIONS_NTF_NoCadr_Title"),
                            SizeIsNtf = false,
                            DashSpace = false                            
                        }
                    });
                return;
            }

            for (var i = 0; i < dt.Rows.Count; i++)
            {
                var dr = dt.Rows[i];
                w.Write("<div>");
                w.Write(dr["Должность"]);
                if ((byte) dr["Совместитель"] == 1) w.Write($" ({dir.Resx.GetString("DIRECTIONS_lPoohBah")})");

                if (employee.OrganizationId.HasValue)
                {
                    var personId = (int) dr["КодЛицаКомпанииСотрудника"];
                    if (!SotrudnikCompanies.Contains(personId))
                        SotrudnikCompanies.Add(personId);

                    w.Write(" ");
                    page.RenderLinkPerson(w, $"linkPostPerson{i}_{personId}", personId.ToString(),
                        dr["Организация"].ToString(), NtfStatus.Empty, false);
                }


                w.Write("</div>");
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
            if (dir == null) return;
            if (dir.SotrudnikField.ValueString.Length == 0)
                return;

            var wr = new StringWriter();
            var wps = dir.Sotrudnik.Workplaces;

            foreach (var wp in wps)
            {
                if (dir.WorkPlaceField.ValueString.Length > 0 && wp.Id == dir.WorkPlaceField.ValueString) continue;
                var containerId = Guid.NewGuid().ToString();
                string icon;
                string title;

                wp.GetLocationSpecifications(out icon, out title);

                title = page.Resx.GetString(title);

                title = wp.IsOrganized
                    ? $"{title} - {dir.Resx.GetString("DIRECTIONS_Msg_РаботаОрганизована")}"
                    : $"{title} - {dir.Resx.GetString("DIRECTIONS_Msg_РаботаНеОрганизована")}";

                wr.Write("<div style=\"margin-left:30px;\">");
                page.RenderLinkLocation(wr, wp.Id, wp.Id, wp.Name, NtfStatus.Empty, "открыть расположение", "", "emplwp");
                if (icon.Length > 0)
                {
                    wr.Write("&nbsp;<img width=\"10\" height=\"10\" src=\"/styles/{0}\" title=\"{1}\"  border=0 {2}/>",
                        icon,
                        title,
                        dir.WorkPlaceField.ValueString.Length == 0 || dir.WorkPlaceField.ValueString.Length > 0 &&
                        wp.Id != dir.WorkPlaceField.ValueString
                            ? $"style =\"cursor:pointer\" onclick=\"directions_openEquipment({wp.Id},'{containerId}');\""
                            : ""
                    );
                }
                else
                {
                    if ((dir.WorkPlaceField.ValueString.Length == 0 || dir.WorkPlaceField.ValueString.Length > 0 &&
                         wp.Id != dir.WorkPlaceField.ValueString) && wp.ExistEquipmentsIt)
                        wr.Write(
                            "&nbsp;<img width=\"10\" height=\"10\" src=\"/styles/detail.gif\" title=\"{0}\" style=\"cursor:pointer\" onclick=\"directions_openEquipment({1},'{2}');\" border=0/>",
                            dir.Resx.GetString("DIRECTIONS_Title_ShowEq"), wp.Id, containerId);
                }

                wr.Write("</div>");
                wr.Write(
                    $"<div></div><div id=\"{containerId}\" style=\"display:none; margin-left: 30px; text-align:left;font-weight:normal !important;\"></div>");
            }

            var text = wr.ToString();
            if (text.Length > 0)
            {
                text = $"{dir.Resx.GetString("DIRECTIONS_Lbl_WpsEpl")}: {text}";
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

            if (!dir.Sotrudnik.HasAccountValid && string.IsNullOrEmpty(dir.Sotrudnik.CommonEmployeeID) && wps.Any(x =>
                    x.IsOrganized
                    && x.IsComputeredWorkPlace
                    && (dir.WorkPlaceField.ValueString.Length == 0 || dir.WorkPlaceField.ValueString != x.Id)
                )
            )
            {
                text =
                    $"{"среди рабочих мест сотрудника есть огранизованное компьютеризированное рабочее место, но сотрудник не имеет учетной записи"}";
                page.RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message = text,
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = dir.Resx.GetString("DIRECTIONS_NTF_ExcessEq_Title")
                        }
                    });
            }

            var listEq = dir.Sotrudnik.EmployeeEquipmentsAnotherWorkPlaces(dir.WorkPlaceField.ValueString);

            if (listEq.Count > 0)
            {
                wr = new StringWriter();
                wr.Write(
                    "{0} <img src=\"/styles/detail.gif\" width=\"10\" height=\"10\" style=\"cursor:pointer\" onclick=\"directions_openAnotherEquipment();\" title=\"{1}\"/>",
                    dir.Resx.GetString("DIRECTIONS_NTF_ExcessEq"), dir.Resx.GetString("DIRECTIONS_Title_ShowEq"));
                page.RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message = wr.ToString(),
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = dir.Resx.GetString("DIRECTIONS_NTF_ExcessEq_Title")
                        }
                    });
            }
        }

        /// <summary>
        ///     Вывод информации о непосредственном руководителе
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
        public void Supervisor(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir == null) return;
            var employee = dir.Sotrudnik;
            if (employee == null || employee.Unavailable)
            {
                w.Write("");
                return;
            }


            var poss = employee.GetUserPositions(CombineType.Неважно);
            if (poss.Rows.Count == 0) return;

            var ws = new StringWriter();
            var sData = dir.SupervisorData;
            if (sData.Rows.Count == 0)
            {
                page.RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message = dir.Resx.GetString("DIRECTIONS_NTF_NoLider1"),
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = dir.Resx.GetString("DIRECTIONS_NTF_NoLider1_Title")
                        }
                    });
                return;
            }

            ws.Write("<div class=\"disp_inlineBlock\">");
            ws.Write("<div class=\"disp_inlineBlock\">");
            ws.Write($"{dir.Resx.GetString("DIRECTIONS_Field_Lider")}:");
            ws.Write("</div> ");

            ws.Write("<div class=\"disp_inlineBlock\">");
            ws.Write("<div class=\"disp_inlineBlock\">");
            page.RenderLinkEmployee(ws, "btnSupervisor", sData.Rows[0]["КодРуководителя"].ToString(),
                page.IsRusLocal
                    ? sData.Rows[0]["Руководитель"].ToString()
                    : sData.Rows[0]["РуководительЛат"].ToString(), NtfStatus.Empty);
            ws.Write("</div> ");

            ws.Write("<div class=\"disp_inlineBlock\">");

            ws.Write(sData.Rows[0]["ДолжностьРуководителя"]);
            if (!sData.Rows[0]["КодЛицаКомпанииСотрудника"].Equals(sData.Rows[0]["КодЛицаКомпанииРуководителя"]))
            {
                ws.Write(" - ");
                page.RenderLinkPerson(ws, "btnSupevisorPerson", sData.Rows[0]["КодЛицаКомпанииРуководителя"].ToString(),
                    sData.Rows[0]["НазваниеКомпанииРуководителя"].ToString());
            }

            ws.Write("</div>");
            ws.Write("</div>");
            ws.Write("</div>");
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
            if (dir == null) return;
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
            if (dir == null) return;
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
        ///     Вывод информации о лице заказчике
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
        public void SotrudnikFinOrg(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir == null) return;
            var employee = dir.Sotrudnik;
            if (employee == null || employee.Unavailable)
            {
                w.Write("");
                return;
            }

            var wr = new StringWriter();


            if (employee.OrganizationId == null)
            {
                wr.Write(dir.Resx.GetString("DIRECTIONS_lbl_ОтветственнаяКомпания") + ": ");

                wr.Write("<font color='red'>");
                wr.Write(dir.Resx.GetString("DIRECTIONS_Msg_СотрудникНетЛицаЗаказчика"));
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

            if (SotrudnikCompanies.Count == 1 && SotrudnikCompanies.Contains(employee.OrganizationId.Value))
            {
                w.Write("");
                return;
            }

            wr.Write(dir.Resx.GetString("DIRECTIONS_lbl_ОтветственнаяКомпания") + ": ");

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
        ///     Отрисовка ссылка на открытие документа в архиве
        /// </summary>
        /// <param name="page">Станица</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Указание</param>
        /// <param name="text">Текст ссылки</param>
        private void DocumentLink(EntityPage page, TextWriter w, Direction dir, string text)
        {
            if (!((DocPage) page).IsInDocView && !((DocPage) page).IsPrintVersion && !dir.IsNew)
            {
                page.RenderLinkDocument(w, int.Parse(dir.Id), text, null, false, NtfStatus.Empty, page.Resx.GetString("cmdOpenDoc"));                
            }
            else
            {
                w.Write(text);
            }
        }

        /// <summary>
        ///     Вывод информации в зависимости от того, что требуется организовать
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
        /// <param name="renderDelete">Выводить ли кнопку удаления выбранного рабочего места</param>
        public void WorkPlaceType(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir == null || dir.SotrudnikField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }

            var bitmask = dir.WorkPlaceTypeField.ValueInt;
           
            if (bitmask == (int) DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто)
            {
                WorkPlaces(page, w, dir, true);
                ValidationMessages.CheckAlreadyMoved(page, w, dir);
                ValidationMessages.CheckSotrudnikWorkPlace(page, w, dir);
            }
            else if (bitmask == (int) DirectionsTypeBitEnum.РабочееМестоВОфисе)
            {
                WorkPlaces(page, w, dir, false);
                ValidationMessages.CheckSotrudnikWorkPlace(page, w, dir);
            }
            else if (bitmask == (int) DirectionsTypeBitEnum.РабочееМестоВнеОфиса)
            {
                w.Write("<div class=\"v4Bold \" style=\"text-align:center;\">");
                DocumentLink(page, w, dir,
                    dir.Resx.GetString(DirectionsTypeBitEnum.РабочееМестоВнеОфиса
                        .GetAttribute<Specifications.DirectionsType>().DocumentTitle));
                w.Write("</div>");

                w.Write("<div style=\"text-align:center; font-weight:normal !important;\">"); 
                ValidationMessages.CheckExistsDirection(page, w, dir);
                w.Write("</div>");
            }
            else if (bitmask == (int) DirectionsTypeBitEnum.УчетнаяЗаписьСотрудникаГруппы)
            {
                w.Write("<div class=\"v4Bold\" style=\"text-align:center;\">");
                DocumentLink(page, w, dir,
                    dir.Resx.GetString(DirectionsTypeBitEnum.УчетнаяЗаписьСотрудникаГруппы
                        .GetAttribute<Specifications.DirectionsType>().DocumentTitle));
                w.Write(" ");
                w.Write("<span id=\"divCommonEmployee\">");
                CommonEmployee(page, w, dir, true);
                w.Write("</span>");
                w.Write("</div>");

                w.Write("<div style=\"text-align:center; font-weight:normal !important;\">");
                ValidationMessages.CheckExistsDirection(page, w, dir);
                w.Write("</div>");
            }
            else if (bitmask == (int) DirectionsTypeBitEnum.ИзменениеУчетнойЗаписи)
            {
                w.Write("<div class=\"v4Bold\" style=\"text-align:center;\">");
                DocumentLink(page, w, dir,
                    dir.Resx.GetString(DirectionsTypeBitEnum.ИзменениеУчетнойЗаписи
                        .GetAttribute<Specifications.DirectionsType>().DocumentTitle));
                w.Write("</div>");

                w.Write("<div style=\"text-align:center; font-weight:normal !important;\">");
                ValidationMessages.CheckExistsDirection(page, w, dir);
                w.Write("</div>");
            }
            else if (bitmask == (int)DirectionsTypeBitEnum.УчетнаяЗаписьНаГостевомМесте)
            {
                w.Write("<div class=\"v4Bold\" style=\"text-align:center;\">");
                DocumentLink(page, w, dir,
                    dir.Resx.GetString(DirectionsTypeBitEnum.УчетнаяЗаписьНаГостевомМесте
                        .GetAttribute<Specifications.DirectionsType>().DocumentTitle));
                w.Write("</div>");

                w.Write("<div style=\"text-align:center; font-weight:normal !important;\">");
                ValidationMessages.CheckExistsDirection(page, w, dir);
                w.Write("</div>");
            }
            else if (bitmask > 0)
            {
                w.Write(
                    $"<div class=\"v4Bold\" style=\"text-align:center;color:red;\"> {"Данный вид указания не поддерживается"}</div>");
            }
            else
            {
                w.Write("");
            }
        }


        /// <summary>
        ///     Вывод информации в зависимости от того, что требуется организовать
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
        /// <param name="isTransfer">Переезд</param>
        public void WorkPlaces(EntityPage page, TextWriter w, Direction dir, bool isTransfer)
        {
            if (dir == null) return;
            if (dir.WorkPlaceField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }

            var ntf = new List<Notification>();
            var containerId = Guid.NewGuid().ToString();

            var iconString = "";
            var detailsString = "";

            w.Write("<div class=\"v4Bold\" style=\"text-align:center;\">");

            if (isTransfer)
                DocumentLink(page, w, dir,
                    dir.Resx.GetString(DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто
                        .GetAttribute<Specifications.DirectionsType>().DocumentTitle) + " ");

            else
                DocumentLink(page, w, dir,
                    dir.Resx.GetString(DirectionsTypeBitEnum.РабочееМестоВОфисе
                        .GetAttribute<Specifications.DirectionsType>().DocumentTitle) + " ");


            #region WorkPlace
            
            var l = dir.LocationWorkPlace;

            if (l == null || l.Unavailable)
            {
                w.Write("#");
                w.Write(dir.WorkPlaceField.ValueString);
                ntf.Add(new Notification
                {
                    Message = dir.Resx.GetString("DIRECTIONS_Msg_РабочееНеДоступно"),
                    Status = NtfStatus.Error,
                    SizeIsNtf = false,
                    DashSpace = false,
                    Description = dir.Resx.GetString("DIRECTIONS_Msg_РабочееНеДоступно_Title")
                });
                page.RenderNtf(w, ntf);
            }
            else
            {
                var label = l.NamePath1;
                l.GetLocationSpecifications(out var icon, out var title);

                title = page.Resx.GetString(title);
                title = l.IsOrganized
                    ? $"{title} - {dir.Resx.GetString("DIRECTIONS_Msg_РаботаОрганизована")}"
                    : $"{title} - {dir.Resx.GetString("DIRECTIONS_Msg_РаботаНеОрганизована")}";

                if (icon.Length > 0)
                    iconString =
                        $"&nbsp;<img width=\"10\" height=\"10\" src=\"/styles/{icon}\" title=\"{title}\" border=0 {$"style=\"cursor:pointer\" onclick=\"directions_openEquipment({l.Id},'{containerId}');\""}/>";
                else if (l.ExistEquipmentsIt)
                    detailsString =
                        $"&nbsp;<img width=\"10\" height=\"10\" src=\"/styles/detail.gif\" title=\"{dir.Resx.GetString("DIRECTIONS_Title_ShowEq")}\" style=\"cursor:pointer\" onclick=\"directions_openEquipment({l.Id},'{containerId}');\" border=0/></br>";

                w.Write("<div class=\"disp_inlineBlock\" style=\"text-align:left;\">");

                page.RenderLinkLocation(w, l.Id, l.Id, label, NtfStatus.Empty, "открыть расположение", "", "wps");

                if (iconString.Length > 0)
                    w.Write(iconString);

                if (detailsString.Length > 0)
                    w.Write(detailsString);

                w.Write(
                    $"<div></div><div id=\"{containerId}\" style=\"display:none; text-align:left;font-weight:normal !important;\"></div>");
                w.Write("</div>");
            }
           
            #endregion


            #region WorkPlaceTo

            var containerIdTo = "";

            if (isTransfer && dir.WorkPlaceToField.ValueString.Length > 0)
            {
                containerIdTo = Guid.NewGuid().ToString();

                var lTo = dir.LocationWorkPlaceTo;

                if (lTo == null || lTo.Unavailable)
                {
                    w.Write("#");
                    w.Write(dir.WorkPlaceToField.ValueString);
                    ntf.Clear();
                    ntf.Add(new Notification
                    {
                        Message = dir.Resx.GetString("DIRECTIONS_Msg_РабочееНеДоступно"),
                        Status = NtfStatus.Error,
                        SizeIsNtf = false,
                        DashSpace = false,
                        Description = dir.Resx.GetString("DIRECTIONS_Msg_РабочееНеДоступно_Title")
                    });
                    page.RenderNtf(w, ntf);
                }
                else
                {
                    iconString = "";
                    detailsString = "";

                    lTo.GetLocationSpecifications(out var icon, out var title);

                    title = page.Resx.GetString(title);
                    title = lTo.IsOrganized
                        ? $"{title} - {dir.Resx.GetString("DIRECTIONS_Msg_РаботаОрганизована")}"
                        : $"{title} - {dir.Resx.GetString("DIRECTIONS_Msg_РаботаНеОрганизована")}";

                    if (icon.Length > 0)
                        iconString =
                            $"&nbsp;<img width=\"10\" height=\"10\" src=\"/styles/{icon}\" title=\"{title}\" border=0 {$"style=\"cursor:pointer\" onclick=\"directions_openEquipment({lTo.Id},'{containerIdTo}');\""}/>";
                    else if (lTo.ExistEquipmentsIt)
                        detailsString =
                            $"&nbsp;<img width=\"10\" height=\"10\" src=\"/styles/detail.gif\" title=\"{dir.Resx.GetString("DIRECTIONS_Title_ShowEq")}\" style=\"cursor:pointer\" onclick=\"directions_openEquipment({lTo.Id},'{containerIdTo}');\" border=0/><br/>";

                    w.Write("<div class=\"disp_inlineBlock\" style=\"text-align:left;\">");
                    w.Write("&nbsp;с ");

                    page.RenderLinkLocation(w, lTo.Id, lTo.Id, lTo.NamePath1, NtfStatus.Empty, "открыть расположение", "", "wps");

                    if (iconString.Length > 0)
                        w.Write(iconString);

                    if (detailsString.Length > 0)
                        w.Write(detailsString);

                    if (!dir.Finished && !lTo.IsComputeredWorkPlace)
                        ntf.Add(new Notification
                        {
                            Message = dir.Resx.GetString("DIRECTIONS_Msg_НеКомпьютеризированноеРабочееМесто"),
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = dir.Resx.GetString("DIRECTIONS_Msg_НеКомпьютеризированноеРабочееМесто_Title")
                        });

                    w.Write(
                        $"<div></div><div id=\"{containerIdTo}\" style=\"display:none; text-align:left;font-weight:normal !important;\"></div>");
                    w.Write("</div>");
                }
            }

            #endregion

            w.Write("</div>");

            
            #region Проверки WorkPlace

            w.Write("<div  class=\"marginL\" style=\"font-weight:normal !important; text-align:center;\">");
            ValidationMessages.CheckLocationWorkPlaceIsOffice(page, w, dir);
            ValidationMessages.CheckLocationWorkPlaceIsComputered(page, w, dir);
                        
            if (!isTransfer) 
                 ValidationMessages.CheckExistsWorkPlaceIsComputeredNoOrganized(page, w, dir, l);
               

            ValidationMessages.CheckEmployeesOnWorkPlace(page, w, dir);            
            ValidationMessages.CheckExistsDirection(page, w, dir);
            w.Write("</div>");

            #endregion
  

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
            if (dir == null) return "";
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
                empls = " [" + dir.Resx.GetString("DIRECTIONS_Msg_OpenEquipNotResp") + "]";
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
            if (dir == null) return;
            var w = new StringWriter();

            var l = page.GetObjectById(typeof(Location), idLocation) as Location;
            if (l == null || l.Unavailable)
            {
                w.Write($"<div style=\"margin-left: 5px;\">{dir.Resx.GetString("DIRECTIONS_NTF_NoEquipment")}</div>");
                return;
            }

            var listEq = l.EquipmentsIt;

            w.Write("<div style=\"margin-left: 10px; margin-bottom:5px;\">");

            if (listEq.Count > 0)
                listEq.OrderBy(x => x.TypeName).ToList().ForEach(delegate(Equipment eq)
                {
                    w.Write("<div style=\"white-space:nowrap;\">");

                    page.RenderLinkEquipment(w, $"eq_{eq.Id}", eq.Id.ToString(), $"{eq.TypeName} {eq.ModelName}", NtfStatus.Empty, dir.Resx.GetString("DIRECTIONS_Title_ShowEq1"), "", "eqinplace");

                    if (eq.EmployeeId != dir.SotrudnikField.ValueInt
                        && (string.IsNullOrEmpty(dir.Sotrudnik.CommonEmployeeID) ||
                            !string.IsNullOrEmpty(dir.Sotrudnik.CommonEmployeeID) &&
                            eq.EmployeeId.ToString() != dir.Sotrudnik.CommonEmployeeID))
                    {
                        w.Write(" [");

                        if (eq.EmployeeId != 0)
                            page.RenderLinkEmployee(w, "eml_" + eq.Id, eq.EmployeeId.ToString(), eq.EmployeeName, NtfStatus.Error);
                        else
                            w.Write(dir.Resx.GetString("DIRECTIONS_Msg_OpenEquipNotResp"));

                        w.Write("]");
                    }

                    w.Write("</div>");
                });
            else
                w.Write($"<div>{dir.Resx.GetString("DIRECTIONS_NTF_NoEquipment")}.</div>");

            w.Write("</div>");

            page.JS.Write("var objD = document.getElementById('{0}'); if(objD){{objD.innerHTML='{1}';}}",
                idContainer,
                HttpUtility.JavaScriptStringEncode(w.ToString()));
            page.JS.Write($"$(\"#{idContainer}\").css(\"display\",\"inline-block\");");
        }


        /// <summary>
        ///     Оборудование сотрудника на чужих рабочих местах
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="idContainer">Идентификтор html-контейнера, куда выводить информацию</param>
        /// <param name="dir">Текущий документ</param>
        public void EquipmentAnotherPlace(EntityPage page, string idContainer, Direction dir)
        {
            if (dir == null) return;
            var w = new StringWriter();
            var listEq = dir.Sotrudnik.EmployeeEquipmentsAnotherWorkPlaces(dir.WorkPlaceField.ValueString);

            w.Write("<div style=\"margin-left: 10px; margin-bottom:5px;\">");
            if (listEq.Count > 0)
                listEq.OrderBy(x => x.TypeName).ToList().ForEach(delegate(Equipment eq)
                {
                    w.Write("<div>");
                    page.RenderLinkEquipment(w, $"eq_{eq.Id}", eq.Id, $"{eq.TypeName} {eq.ModelName}", NtfStatus.Empty, dir.Resx.GetString("DIRECTIONS_Title_ShowEq1"), "", "advloceq");
                                       
                    w.Write(" -> ");

                    if (eq.LocationId == 0) return;
                    page.RenderLinkLocation(w, $"loc_{eq.LocationId}", eq.LocationId.ToString(), eq.LocationName, NtfStatus.Empty, "открыть расположение","","advloc");                    
                    w.Write("</div>");
                });
            else
                w.Write(dir.Resx.GetString("DIRECTIONS_Title_EquipmentNoLocation3"));

            w.Write("</div>");

            page.JS.Write("var obj = document.getElementById('{0}'); if(obj) obj.innerHTML='{1}';",
                idContainer,
                HttpUtility.JavaScriptStringEncode(w.ToString()));
            page.JS.Write($"$(\"#{idContainer}\").css(\"display\",\"inline-block\");");
        }

        /// <summary>
        ///     Вывод проверок значения в поле e-mail
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
        public void EmailCheck(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir == null) return;
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
                            Description = dir.Resx.GetString("DIRECTIONS_Msg_EmailНеКорректен_Title")
                        }
                    );
            }

            ValidationMessages.CheckUniqueEmail(page, dir, ntfs);
            
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
                            Description = dir.Resx.GetString("DIRECTIONS_NTF_EmailName_Title")
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
                            Description = dir.Resx.GetString("DIRECTIONS_NTF_EmailDomain_Title")
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
            if (dir == null) return;
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
                        Description = dir.Resx.GetString("DIRECTIONS_Msg_ЛогинНеКорректен_Title")
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
                        Description = dir.Resx.GetString("DIRECTIONS_Msg_ЛогинИмя_Title")
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
                        Description = dir.Resx.GetString("DIRECTIONS_Msg_ЛогинСуществует_Title")
                    });
            }

            ValidationMessages.CheckEmailInCorporateDomain(page, dir, ntfs);

            if (ntfs.Count > 0 && render)
                page.RenderNtf(w, ntfs);
        }

        /// <summary>
        ///     Вывод информации о группе(название и ссылка) сотрудника
        /// </summary>
        /// <param name="page">Станица документа</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Объект документа</param>
        public void CommonEmployee(EntityPage page, TextWriter w, Direction dir, bool withOutLabel = false, NtfStatus status = NtfStatus.Empty)
        {
            if (dir == null) return;
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
            if (!withOutLabel) w.Write($"{dir.Resx.GetString("DIRECTIONS_NTF_ГруппаПосменнойРаботы")}: ");
            page.RenderLinkEmployee(w, "linkCommonEmployee", commonEmployee, status, false);
            w.Write(
                "&nbsp;<img width=\"10\" height=\"10\" src=\"/styles/detail.gif\" title=\"{0}\" style=\"cursor:pointer\" onclick=\"directions_groupMembers({1});\" border=0/>",
                dir.Resx.GetString("DIRECTIONS_Title_GroupMembers"), commonEmployee.Id);
            w.Write(
                $"<div></div><div id=\"divInfoGroup_{commonEmployee.Id}\" style=\"display:none; margin-left: 30px; \"></div>");
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
            if (dir == null) return;
            var w = new StringWriter();

            var employee = new Employee(idGroup);

            var listEq = employee.GroupMembers;

            w.Write("<div style=\"text-align:left !important; font-weight:normal !important;\">");

            if (listEq.Count > 0)
                listEq.OrderBy(x => x.FIO).ToList().ForEach(delegate(Employee gm)
                {
                    w.Write("<div style=\"white-space:nowrap;\">");
                    page.RenderLinkEmployee(w, $"linkGroupMembers{gm.Id}", gm, NtfStatus.Empty, false);
                    w.Write("</div>");
                });
            else
                w.Write($"<div>{dir.Resx.GetString("DIRECTIONS_NTF_EmptyGroup")}.</div>");

            w.Write("</div>");

            page.JS.Write("var obj_{0} = document.getElementById('{0}'); if(obj_{0}){{obj_{0}.innerHTML='{1}';}}",
                idContainer,
                HttpUtility.JavaScriptStringEncode(w.ToString()));

            page.JS.Write($"$(\"#{idContainer}\").css(\"display\",\"inline-block\");");
        }

        public void AdvancedGrants(EntityPage page, TextWriter w, Direction dir, int bitMask)
        {
            if (dir == null) return;
            if (dir.WorkPlaceTypeField.ValueInt == 0) return;

            var ags = dir.AdvancedGrantsAvailable.FindAll(x => x.RefersTo == bitMask);
            var inx = 0;
            ags.OrderBy(x => x.OrderOutput).ToList().ForEach(delegate(AdvancedGrant grant)
            {
                var agChecked = dir.PositionAdvancedGrants.Any(x => x.GrantId.ToString() == grant.Id);
                var bit = (int) Math.Pow(2, dir.WorkPlaceTypeField.ValueInt - 1);

                if (!agChecked)
                {
                    if (grant.TaskChoose == 0) return;
                    if ((grant.TaskChoose & bit) != bit) return;
                }

                if (!agChecked && dir.IsNew && dir.WorkPlaceTypeField.ValueInt > 0 && grant.TaskDefault > 0)
                    agChecked = (grant.TaskDefault & bit) == bit;

                var agDisabled = grant.TaskChoose == 0 || (grant.TaskChoose & bit) != bit;
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
        public string ADSI_RenderInfoByEmployee(EntityPage page, TextWriter w, Direction dir, List<Notification> ntfs, Employee empl, int bitmask)
        {            
            string adsiPath;
            if (int.Parse(page.CurrentUser.Id) != (int) КодыСотрудников.Анисимов) return "";

            ADSI_RenderPath(page, w, empl.ADSIAccountPath(page.CurrentUser), out adsiPath, false, false);
            ADSI_RenderAccountExpires(page, dir, ntfs,  empl, empl.AccountExpires, bitmask);
            ADSI_RenderDisabled(page, dir, ntfs, empl, empl.AccountDisabled, bitmask);
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
        private void ADSI_RenderDisabled(EntityPage page, Direction dir, List<Notification> ntfs,
            Employee empl,
            int? adsiDisabled, int bitMask)
        {
            
            if (!adsiDisabled.HasValue || adsiDisabled.HasValue && adsiDisabled.Value == 0) return;

            ntfs.Add(new Notification
            {
                Message = "Disabled",
                Status = (bitMask & 2) == 2 && empl.Status == 3 ? NtfStatus.Empty : NtfStatus.Error,
                SizeIsNtf = false,
                DashSpace = false,
                Description = dir.Resx.GetString("DIRECTIONS_NTF_Disabled_Title")
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
        private void ADSI_RenderAccountExpires(EntityPage page, Direction dir, List<Notification> ntfs,            
            Employee empl, DateTime? accountExpires, int bitMask)
        {
            
            var accountExpiresIsDbNull = !accountExpires.HasValue;
            if (accountExpiresIsDbNull && (bitMask & 2) == 2 && empl.Status == 3 && empl.HasAccount_)
                ntfs.Add(new Notification
                {
                    Message = dir.Resx.GetString("DIRECTIONS_NTF_AccountExpiresNo"),
                    Status = NtfStatus.Error,
                    SizeIsNtf = false,
                    DashSpace = false,
                    Description = dir.Resx.GetString("DIRECTIONS_NTF_AccountExpiresNo_Title")
                });
            else if (!accountExpiresIsDbNull && (bitMask & 1) == 1)
                ntfs.Add(new Notification
                {
                    Message = "AccountExpires: " + accountExpires.Value.ToString("dd.MM.yy"),
                    Status = NtfStatus.Error,
                    SizeIsNtf = false,
                    DashSpace = false,
                    Description = dir.Resx.GetString("DIRECTIONS_NTF_AccountExpires_Title")
                });
            else if (!accountExpiresIsDbNull && (bitMask & 2) == 2)
                ntfs.Add(new Notification
                {
                    Message = "AccountExpires: " + accountExpires.Value.ToString("dd.MM.yy"),
                    Status = (bitMask & 2) == 2 && empl.Status == 3 ? NtfStatus.Empty : NtfStatus.Error,
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
        private void ADSI_RenderPath(EntityPage page, TextWriter w, string path, out string adsiPath,
            bool marginL = true, bool streamWrite = true)
        {

            if (string.IsNullOrEmpty(path))
            {
                adsiPath = "";
                return;
            }

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