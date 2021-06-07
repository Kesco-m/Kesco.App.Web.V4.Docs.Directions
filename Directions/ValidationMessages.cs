using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.BaseExtention.Enums.Docs;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities;
using Kesco.Lib.Entities.Corporate;
using Kesco.Lib.Entities.Documents.EF.Directions;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Web.Controls.V4.Globals;
using Kesco.Lib.Web.Settings;
using Convert = Kesco.Lib.ConvertExtention.Convert;

namespace Kesco.App.Web.Docs.Directions
{
    /// <summary>
    ///     Класс реализующий все проверки данных указания на организацию работы
    /// </summary>
    public class ValidationMessages
    {
        /// <summary>
        ///     Проверка существования других указаний
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static int CheckExistsDirection(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.Finished)
            {
                w.Write("");
                return -1;
            }
            if (dir.WorkPlaceTypeField.ValueString.Length > 0
                 && dir.WorkPlaceTypeField.ValueInt == (int)DirectionsTypeBitEnum.РабочееМестоВОфисе && dir.WorkPlaceField.ValueString.Length==0)
                return -1;

            var idDoc = -1;
            var sqlParams = new Dictionary<string, object>
            {
                {"@КодСотрудника", dir.SotrudnikField.Value},
                {"@КодРасположения1", dir.WorkPlaceField.Value},
                {"@КодРасположения2", dir.WorkPlaceToField.Value},
                {"@ЧтоОрганизовать", dir.WorkPlaceTypeField.Value},
                {"@КодДокумента", dir.DocId}
            };
            using (
                var dbReader = new DBReader(SQLQueries.SELECT_2356_CHECK_ДругиеУказанияНаСотрудника, CommandType.Text,
                    Config.DS_document, sqlParams))
            {
                if (dbReader.HasRows)
                {
                    var colКодДокумента = dbReader.GetOrdinal("КодДокумента");
                    var colДокумент = dbReader.GetOrdinal("Документ");

                    var list = new List<Notification>();

                    while (dbReader.Read())
                    {
                        var wr = new StringWriter();
                        if (idDoc == -1) idDoc = dbReader.GetInt32(colКодДокумента);
                        page.RenderLinkDocument(wr, dbReader.GetInt32(colКодДокумента), dbReader.GetString(colДокумент), null, false, NtfStatus.Error, dir.Resx.GetString("DIRECTIONS_Msg_ExistsDirection_Title"));
                        
                        list.Add(new Notification
                        {
                            Message = wr.ToString(),
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = dir.Resx.GetString("DIRECTIONS_Msg_ExistsDirection_Title"),
                            CSSClass = "v4Normal"
                        });
                    }

                    page.RenderNtf(w, list);
                }
                else
                {
                    w.Write("");
                }
            }
            return idDoc;
        }

        /// <summary>
        ///     Проверка наличия нескольких сотрудников на одном рабочем месте
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static void CheckEmployeesOnWorkPlace(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.Sotrudnik.Unavailable ||
                dir.WorkPlaceField.ValueString.Length == 0 || dir.Sotrudnik.GroupMembers.Count > 0 || dir.Finished)
            {
                w.Write("");
                return;
            }

            var employee = dir.Sotrudnik;

            var listEmpl = employee.EmployeesOnWorkPlace(dir.WorkPlaceField.ValueInt);
            var emplIds = new StringCollection();

            if (listEmpl.Count > 0)
            {
                var wr = new StringWriter();
                var ntfStatus = NtfStatus.Empty;

                bool parent_onwp = false;

                if (dir.SotrudnikParentCheckField.ValueString.Length > 0 && dir.SotrudnikParentCheckField.ValueInt == 2
                    && dir.SotrudnikParentField.ValueString.Length > 0 && !dir.SotrudnikParent.Unavailable)
                {
                    var parent_wps = dir.SotrudnikParent.Workplaces;
                    parent_onwp = parent_wps.Any(x => x.Id == dir.WorkPlaceField.ValueString);
                }

                //проверяем: не сидит ли кто-то еще на этом месте
                foreach (var empl in listEmpl)
                {
                    if (parent_onwp && dir.SotrudnikParentField.ValueString == empl.Id) continue;
                    emplIds.Add(empl.Id);
                }
                

                //если никто не сидит, кроме сотрудника вместо и сотрудника указания или рабочее место недоступно
                if (emplIds.Count == 0 || dir.LocationWorkPlace.Unavailable)
                {
                    w.Write("");
                    return;
                }
                else
                {
                    if (dir.LocationWorkPlace.IsComputeredWorkPlace)
                    {
                        wr.Write(dir.Resx.GetString("DIRECTIONS_NTF_ПосменнаяРабота") + ": ");
                        ntfStatus = NtfStatus.Error;
                    }
                    else
                        wr.Write(dir.Resx.GetString("DIRECTIONS_NTF_РабМестаВместе") + ": ");

                }
            
                
                var inx = 1;
                var existCommonEmpl = false;
                foreach (var empl in listEmpl)
                    if (!string.IsNullOrEmpty(employee.CommonEmployeeID) && empl.Id == employee.CommonEmployeeID)
                    {
                        existCommonEmpl = true;
                        break;
                    }

                if (existCommonEmpl)
                {
                    w.Write("");
                    return;
                }

                foreach (var empl in listEmpl)
                {
                    if (!string.IsNullOrEmpty(empl.CommonEmployeeID)) continue;
                    if (!emplIds.Contains(empl.Id)) continue;

                    page.RenderLinkEmployee(wr, "", empl, ntfStatus, false);
                    if (inx < listEmpl.Count) wr.Write("; ");
                    inx++;
                }

                page.RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message = wr.ToString(),
                            Status = ntfStatus,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = ntfStatus == NtfStatus.Error
                                ? dir.Resx.GetString("DIRECTIONS_NTF_ПосменнаяРабота_Title")
                                : ""
                        }
                    });
            }
            else
            {
                w.Write("");
            }
        }

      
        /// <summary>
        ///     Проверка, что указанное рабочее место есть среди рабочих мест сотрудника
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static void CheckSotrudnikWorkPlace(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.LocationWorkPlace == null || dir.LocationWorkPlace.Unavailable || dir.Sotrudnik.Unavailable || dir.Finished)
            {
                w.Write("");
                return;
            }

            var workplaces = dir.Sotrudnik.Workplaces;

            if (workplaces == null || workplaces.Count == 0)
            {
                w.Write("");
                return;
            }

            if (!workplaces.Any(x => x.Id.Equals(dir.LocationWorkPlace.Id)))
                page.RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message = dir.Resx.GetString("DIRECTIONS_Msg_СотрудникНетРабМеста_2_0"),
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = dir.Resx.GetString("DIRECTIONS_Msg_РабочееМестоНеСоответствует_Title"),
                            CSSClass = "v4Normal"
                        }
                    });

            else
                w.Write("");
        }

        /// <summary>
        /// Проверка того, что сотрудник уже переехал на новое рабочее место
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static void CheckAlreadyMoved(EntityPage page, TextWriter w, Direction dir)
        {

            if (dir.SotrudnikField.ValueString.Length == 0 || dir.Sotrudnik.Unavailable || dir.WorkPlaceField.ValueString.Length==0 || dir.Finished)
            {
                w.Write("");
                return;
            }

            if (dir.Sotrudnik.Workplaces.Any(x => x.Id == dir.WorkPlaceField.ValueString && x.IsOrganized))
                page.RenderNtf(w, new List<Notification>
                                    {
                                                new Notification
                                                {
                                                    Message = dir.Resx.GetString("DIRECTIONS_NTF_УжеПереехал"),
                                                    Status = NtfStatus.Recommended,
                                                    SizeIsNtf = false,
                                                    DashSpace = false,
                                                    Description = dir.Resx.GetString("DIRECTIONS_NTF_УжеПереехал_Title")
                                                }
                                    });
            else
                w.Write("");

        }

        /// <summary>
        ///     Проверка наличия у сотрудника компьютеризированного рабочего места в офисе
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        /// <param name="wpId">Идентификатор рабочего места</param>
        public static void CheckExistsWorkPlaceIsComputeredNoOrganized(EntityPage page, TextWriter w, Direction dir, Location l)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.Sotrudnik.Unavailable || l == null || l.Unavailable || dir.Finished)
            {
                w.Write("");
                return;
            }

            if (dir.Sotrudnik.Workplaces.Any(x => x.IsComputeredWorkPlace && !x.IsOrganized && x.Id != l.Id))
                page.RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message = dir.Resx.GetString("DIRECTIONS_Msg_УСотрудникаЕстьРабМесто"),
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = dir.Resx.GetString("DIRECTIONS_Msg_УСотрудникаЕстьРабМесто_Title")
                        }
                    });
            else
                w.Write("");
        }

        /// <summary>
        ///     Проверка, что рабочее местов в офисе
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static void CheckLocationWorkPlaceIsOffice(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.WorkPlaceField.ValueString.Length == 0 || dir.LocationWorkPlace==null || dir.LocationWorkPlace.Unavailable  || dir.Finished)
            {
                w.Write("");
                return;
            }

            if (!dir.LocationWorkPlace.IsOffice)
                page.RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message = dir.Resx.GetString("DIRECTIONS_Msg_РабочееМестоНеВОфисе"),
                            Status = NtfStatus.Recommended,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = dir.Resx.GetString("DIRECTIONS_Msg_РабочееМестоНеВОфисе_Title")
                        }
                    });
            else
                w.Write("");
        }

        /// <summary>
        ///     Порверка, что рабочее место компьютеризированное
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static void CheckLocationWorkPlaceIsComputered(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.WorkPlaceField.ValueString.Length == 0 || dir.LocationWorkPlace == null || dir.LocationWorkPlace.Unavailable || dir.Finished)
            {
                w.Write("");
                return;
            }

            if (!dir.LocationWorkPlace.IsComputeredWorkPlace && !dir.LocationWorkPlace.IsGuestWorkPlace)
                page.RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message = dir.Resx.GetString("DIRECTIONS_Msg_НеКомпьютеризированноеРабочееМесто"),
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = dir.Resx.GetString("DIRECTIONS_Msg_НеКомпьютеризированноеРабочееМесто_Title")
                        }
                    });
            else
                w.Write("");
        }

        /// <summary>
        ///     Проверка правильности введенного Email
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static void CheckEmailName(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.MailNameField.ValueString.Length == 0 || dir.DomainField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }

            var email = dir.MailNameField.ValueString + "@" + dir.DomainField.ValueString;

            if (!Validation.IsEmail(email))
                w.Write(dir.Resx.GetString("DIRECTIONS_Msg_EmailНеКорректен"));
            else
                w.Write("");
        }


        /// <summary>
        ///     Проверка наличика корпоративного email у других сотрудников
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static void CheckEmailInCorporateDomain(EntityPage page, Direction dir, List<Notification> ntfs)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.Sotrudnik.Unavailable
                                                           || dir.LoginField.ValueString.Length == 0 || dir.Finished)
            {                
                return;
            }

            var email = dir.LoginField.ValueString + "@" + GlobalBase.Domain;          
            var sqlParams = new Dictionary<string, object> { { "@Email", email }, { "@Login", dir.LoginField.ValueString } };

            var dt = DBManager.GetData(SQLQueries.SP_EmailUsed, Config.DS_user, CommandType.StoredProcedure, sqlParams);

            if (dt.Rows.Count == 0) return;           

            Employee empl;
            var bitMask = dir.SotrudnikParentCheckField.ValueInt;

            for (var i = 0; i < dt.Rows.Count; i++)
            {
                using (var w = new StringWriter())
                {
                    var rez = int.Parse(dt.Rows[i]["Используется"].ToString());
                    empl = new Employee(dt.Rows[i]["КодСотрудника"].ToString());

                    if ((bitMask & (int)DirectionsSotrudnikParentBitEnum.ВместоСотрудника) == (int)DirectionsSotrudnikParentBitEnum.ВместоСотрудника
                     && dir.SotrudnikParentField.ValueString.Length > 0
                     && dir.SotrudnikParentField.ValueString == empl.Id) continue;

                    string errMsg;
                    switch (rez)
                    {

                        case 1:
                            errMsg = string.Format(dir.Resx.GetString("DIRECTIONS_NTF_EmailNotUnique_1"), $"{email} ");
                            break;
                        case 2:
                            errMsg = string.Format(dir.Resx.GetString("DIRECTIONS_NTF_EmailNotUnique_2"), $"{email} ");
                            break;
                        case 3:
                            errMsg = string.Format(dir.Resx.GetString("DIRECTIONS_NTF_EmailNotUnique_3"), $"{email} ");
                            break;
                        default:
                            errMsg = string.Format(dir.Resx.GetString("DIRECTIONS_NTF_EmailNotUnique"), $"{email} ");
                            break;
                    }

                    w.Write($"{errMsg} ");
                    page.RenderLinkEmployee(w, "corpEmplEmail" + empl.Id, empl, NtfStatus.Error, false);

                    ntfs.Add(
                        new Notification
                        {
                            Message = w.ToString(),
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = dir.Resx.GetString("DIRECTIONS_Msg_ЛогинСуществует_Title")
                        }
                    );
                }
            }
        }

        /// <summary>
        ///     Проверка введенного Email на уникальность
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static void CheckUniqueEmail(EntityPage page, Direction dir, List<Notification> ntfs)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.Sotrudnik.Unavailable ||
                dir.MailNameField.ValueString.Length == 0 || dir.LoginField.ValueString.Length == 0 || dir.DomainField.ValueString.Length == 0 || dir.Finished)
            {
                return;
            }

            var email = dir.MailNameField.ValueString + "@" + dir.DomainField.ValueString;
            var sqlParams = new Dictionary<string, object> { { "@Email", email }, { "@Login",  dir.LoginField.ValueString } };

            var dt = DBManager.GetData(SQLQueries.SP_EmailUsed, Config.DS_user, CommandType.StoredProcedure, sqlParams);

            if (dt.Rows.Count==0) return;


            Employee empl;
            var bitMask = dir.SotrudnikParentCheckField.ValueInt;

            for (var i = 0; i < dt.Rows.Count; i++)
            {
                using (var w = new StringWriter())
                {
                    var rez = int.Parse(dt.Rows[i]["Используется"].ToString());
                    empl = new Employee(dt.Rows[i]["КодСотрудника"].ToString());

                    if ((bitMask & (int)DirectionsSotrudnikParentBitEnum.ВместоСотрудника) == (int)DirectionsSotrudnikParentBitEnum.ВместоСотрудника 
                        && dir.SotrudnikParentField.ValueString.Length > 0 
                        && dir.SotrudnikParentField.ValueString == empl.Id) continue;

                    string errMsg;
                    switch (rez)
                    {

                        case 1:
                            errMsg = string.Format(dir.Resx.GetString("DIRECTIONS_NTF_EmailNotUnique_1"), $"{email} ");
                            break;
                        case 2:
                            errMsg = string.Format(dir.Resx.GetString("DIRECTIONS_NTF_EmailNotUnique_2"), $"{email} ");
                            break;
                        case 3:
                            errMsg = string.Format(dir.Resx.GetString("DIRECTIONS_NTF_EmailNotUnique_3"), $"{email} ");
                            break;
                        default:
                            errMsg = string.Format(dir.Resx.GetString("DIRECTIONS_NTF_EmailNotUnique"), $"{email} ");
                            break;
                    }

                    w.Write($"{errMsg} ");
                    page.RenderLinkEmployee(w, "emplEmail" + empl.Id, empl, NtfStatus.Error, false);

                    ntfs.Add(
                        new Notification
                        {
                            Message = w.ToString(),
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = dir.Resx.GetString("DIRECTIONS_NTF_EmailNotUnique_Title")
                        }
                    );
                }
            }
        }


        /// <summary>
        ///     Поиск объктов в AD по email
        /// </summary>
        /// <param name="dir">Текущий документ</param>
        /// <param name="email">Почтовый адрес</param>
        /// <param name="emailAddress">Коллекция найденных адресов</param>
        /// <param name="emailUsersGuid">Коллекция guid найденных сотрудников</param>
        private static void ADSIFindEmailsAndAccountByEmail(Direction dir, string email,
            ICollection<string> emailAddress, ICollection<string> emailUsersGuid)
        {
            var result = ADSI.FindAccountByEmail(email);
            if (result == null)
                return;


            var empl = dir.Sotrudnik;
            for (var i = 0; i < result.Properties["objectGUID"].Count; i++)
            {
                var adsiGuid = new Guid((byte[]) result.Properties["objectGUID"][i]);
                if (!empl.Guid.Equals(adsiGuid))
                    emailUsersGuid.Add(adsiGuid.ToString());
            }

            if (emailUsersGuid.Count == 0)
                return;


            for (var i = 0; i < result.Properties["proxyAddresses"].Count; i++)
                if (result.Properties["proxyAddresses"][i].ToString()
                    .StartsWith("SMTP", StringComparison.CurrentCulture))
                    emailAddress.Add(result.Properties["proxyAddresses"][i].ToString().Substring(5).Trim());
        }

        /// <summary>
        ///     Отсисовка списка сотрудников по GUID
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="emailUsersGuid">Коллекция guids сотрудников</param>
        private static void RenderEmployeeListByGuid(EntityPage page, TextWriter w, ICollection<string> emailUsersGuid)
        {
            if (emailUsersGuid.Count > 0)
            {
                w.Write(" [");
                var inx = 0;
                foreach (var t in emailUsersGuid)
                {
                    inx++;
                    var s = new Employee(new Guid(t));
                    if (!s.Unavailable)
                        page.RenderLinkEmployee(w, $"emlc{s.Id}", s, NtfStatus.Error, false);
                    else
                        w.Write(t);

                    if (inx < emailUsersGuid.Count) w.Write("; ");
                }

                w.Write("] ");
            }
        }


        /// <summary>
        ///     Проверка соответствия email имени.фамилии сотрудника
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static void CheckEmailNameSortudnik(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.Sotrudnik.Unavailable
                                                           || dir.MailNameField.ValueString.Length == 0
                                                           || dir.Sotrudnik.IsGroupWork)
            {
                w.Write("");
                return;
            }

            if (!Validation.IsNameInEmail(dir.MailNameField.ValueString, dir.LoginField.ValueString,
                dir.Sotrudnik.FirstNameEn, dir.Sotrudnik.LastNameEn))
                w.Write(dir.Resx.GetString("DIRECTIONS_NTF_EmailName"));
            else
                w.Write("");
        }

        /// <summary>
        ///     Проверка соответствия домена почтового ящика компании сотрудника
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static void CheckEmailDomainByPerson(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.Sotrudnik.Unavailable
                                                           || dir.DomainField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }

            var isDomain = dir.Sotrudnik.CheckEmailDomainName(dir.DomainField.ValueString, dir.DomainNames);

            if (!isDomain)
                w.Write(dir.Resx.GetString("DIRECTIONS_NTF_EmailDomain"));          
        }

        /// <summary>
        ///     Проверка правильности введенного Логина
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static void CheckLogin(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.LoginField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }

            if (!Validation.IsLogin(dir.LoginField.ValueString))
                w.Write(dir.Resx.GetString("DIRECTIONS_Msg_ЛогинНеКорректен"));
            else
                w.Write("");
        }

        /// <summary>
        ///     Проверка соответствия логина имени.фамилии сотрудника
        /// </summary>
        /// <param name="page"></param>
        /// <param name="w"></param>
        /// <param name="dir"></param>
        public static void CheckLoginName(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.Sotrudnik.Unavailable
                                                           || dir.LoginField.ValueString.Length == 0
                                                           || dir.Sotrudnik.IsGroupWork)
            {
                w.Write("");
                return;
            }

            if (!dir.Sotrudnik.CheckLoginName(dir.LoginField.ValueString))
                w.Write(dir.Resx.GetString("DIRECTIONS_Msg_ЛогинИмя"));
            else
                w.Write("");
        }

        /// <summary>
        ///     Проверка уникальности введенного логина
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static void CheckUniqueLogin(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.Sotrudnik.Unavailable
                                                           || dir.LoginField.ValueString.Length == 0 || dir.Finished)
            {
                w.Write("");
                return;
            }

            var sqlParams = new Dictionary<string, object>
            {
                {"@КодСотрудника", dir.SotrudnikField.Value},
                {"@Login", dir.LoginField.Value}
            };
            using (
                var dbReader = new DBReader(SQLQueries.SELECT_2356_CHECK_УникальныйЛогинСотрудника, CommandType.Text,
                    Config.DS_user, sqlParams))
            {
                if (dbReader.HasRows)
                    w.Write(dir.Resx.GetString("DIRECTIONS_Msg_ЛогинСуществует"));
                else
                    w.Write("");
            }
        }


        /// <summary>
        ///     Проверка выбранного сотрудника-родителя
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        /// <param name="ntfList">Список предупреждений</param>
        public static void CheckSotrudnikParent(EntityPage page, TextWriter w, Direction dir,
            List<Notification> ntfList)
        {
            if (dir.SotrudnikParentField.ValueString.Length == 0 ||
                dir.SotrudnikParentCheckField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }

            var p = dir.SotrudnikParent;
            var bitMask = int.Parse(dir.SotrudnikParentCheckField.ValueString);


            if ((bitMask & 1) == 1 && !p.HasAccount_ && !dir.Finished)
                ntfList.Add(new Notification
                {
                    Message = dir.Resx.GetString("DIRECTIONS_Msg_СотрудникНеИмеетЛогина"),
                    Status = NtfStatus.Error,
                    SizeIsNtf = false,
                    DashSpace = false,
                    Description = dir.Resx.GetString("DIRECTIONS_Msg_СотрудникНеИмеетЛогина_Title")
                });

            if ((bitMask & 2) == 2
                && !p.HasAccount_
                && dir.SotrudnikField.ValueString.Length > 0
                && !dir.Sotrudnik.Unavailable
                && !dir.Sotrudnik.HasAccount_
                && !dir.Finished)
                ntfList.Add(new Notification
                {
                    Message = dir.Resx.GetString("DIRECTIONS_Msg_СотрудникНеИмеетЛогина"),
                    Status = NtfStatus.Error,
                    SizeIsNtf = false,
                    DashSpace = false,
                    Description = dir.Resx.GetString("DIRECTIONS_Msg_СотрудникНеИмеетЛогина_Title")
                });

            if (dir.SotrudnikField.ValueString.Length > 0 &&
                dir.SotrudnikField.ValueString.Equals(dir.SotrudnikParentField.ValueString))
                ntfList.Add(new Notification
                {
                    Message = dir.Resx.GetString("DIRECTIONS_NTF_СотрудникСовпадает")?.ToLower(),
                    Status = NtfStatus.Error,
                    SizeIsNtf = false,
                    DashSpace = false
                });

            if (ntfList.Count > 0)
                page.RenderNtf(w, ntfList);
        }

        /// <summary>
        ///     Проверка состояния выбранного сотрудника-родителя
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        /// <param name="ntfList">Список предупреждений</param>
        public static void CheckSotrudnikParentStatus(EntityPage page, TextWriter w, Direction dir,
            List<Notification> ntfList)
        {
            if (dir.SotrudnikParentField.ValueString.Length == 0 ||
                dir.SotrudnikParentCheckField.ValueString.Length == 0 || dir.Finished)
            {
                w.Write("");
                return;
            }

            var p = dir.SotrudnikParent;
            var bitMask = int.Parse(dir.SotrudnikParentCheckField.ValueString);

            var message = "";
            var description = dir.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние_Title");
            switch (p.Status)
            {
                case 0:
                    if ((bitMask & 2) == 2)
                    {
                        message = dir.Resx.GetString("DIRECTIONS_Msg_СотрудникНеУволен");
                        description = dir.Resx.GetString("DIRECTIONS_Msg_СотрудникНеУволен_Title");
                    }

                    break;
                case 1:
                    if ((bitMask & 2) != 2)
                        message = dir.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние1");
                    break;              
                case 3:
                    if ((bitMask & 1) == 1)
                    {
                        message = dir.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние3");
                        description = dir.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние3_Title");
                    }

                    break;
                case 4:
                    message = dir.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние4");
                    break;
                case 5:
                    message = dir.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние5");
                    break;
            }

            if (string.IsNullOrEmpty(message))
            {
                w.Write("");
                return;
            }

            ntfList.Add(new Notification
            {
                Message = message,
                Status = NtfStatus.Error,
                SizeIsNtf = false,
                DashSpace = false,
                Description = description
            });
        }

        /// <summary>
        ///     Проверка выбранного сотрудника
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        /// <param name="ntfList">Список предупреждений</param>
        /// <param name="render">Вывести в поток</param>
        public static void CheckSotrudnik(EntityPage page, TextWriter w, Direction dir, List<Notification> ntfList,
            bool render = true)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.Sotrudnik.Unavailable)
            {
                w.Write("");
                return;
            }

            if (dir.Sotrudnik.PersonEmployeeId == null && !dir.Sotrudnik.IsGroupWork)
                ntfList.Add(new Notification
                {
                    Message = dir.Resx.GetString("DIRECTIONS_Msg_СотрудникНетЛица"),
                    Status = NtfStatus.Error,
                    SizeIsNtf = false,
                    DashSpace = false,
                    Description = dir.Resx.GetString("DIRECTIONS_Msg_СотрудникНетЛица_Title")
                });


            if (ntfList.Count > 0 && render)
                page.RenderNtf(w, ntfList);
        }


        /// <summary>
        ///     Проверка состояния выбранного сотрудника
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        /// <param name="ntfList">Список предупреждений</param>
        public static void CheckSotrudnikStatus(EntityPage page, TextWriter w, Direction dir,
            List<Notification> ntfList)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.SotrudnikField.ValueString.Length == 0 ||
                dir.Sotrudnik.Unavailable || dir.Finished)
            {
                w.Write("");
                return;
            }

            var message = "";
            switch (dir.Sotrudnik.Status)
            {
                //case 1:
                //    message = page.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние1");
                //    break;
                //case 2:
                //    message = page.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние2");
                //    break;
                case 3:
                    message = dir.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние3");
                    break;
                case 4:
                    message = dir.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние4");
                    break;
                case 5:
                    message = dir.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние5");
                    break;
            }

            if (string.IsNullOrEmpty(message))
            {
                w.Write("");
                return;
            }


            ntfList.Add(new Notification
            {
                Message = message,
                Status = NtfStatus.Error,
                DashSpace = false,
                SizeIsNtf = false,
                Description = dir.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние_Title")
            });
        }

        /// <summary>
        ///     Проверка, что на указанном рабочем месте работает группа сотруников
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static void CheckSotrudnikWorkPlaceGroup(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.WorkPlaceField.ValueString.Length == 0 ||
                dir.Finished)
            {
                w.Write("");
                return;
            }

            var sqlParams = new Dictionary<string, object>
            {
                {"@КодСотрудника", dir.SotrudnikField.Value},
                {"@Кодрасположения", dir.WorkPlaceField.Value}
            };

            using (
                var dbReader = new DBReader(SQLQueries.SELECT_2356_CHECK_СотрудникНаМестеГруппы, CommandType.Text,
                    Config.DS_user, sqlParams))
            {
                if (!dbReader.HasRows)
                {
                    w.Write("");
                    return;
                }

                var colСотрудник = dbReader.GetOrdinal("Сотрудник");

                while (dbReader.Read())
                {
                    w.Write(
                        $"{dir.Resx.GetString("DIRECTIONS_NTF_ГруппаНаРасположении")} {dbReader.GetString(colСотрудник)}!");
                    return;
                }
            }
        }

        /// <summary>
        ///     Получение информации о необходимости выдачи Sim-карты
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static void CheckSimInfo(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.Finished)
            {
                w.Write("");
                return;
            }

            var empl = dir.Sotrudnik;

            var post = empl.SimPost;
            var bitmask = dir.PhoneEquipField.ValueString.Length > 0 ? dir.PhoneEquipField.ValueInt : 0;

            if (!empl.SimRequired && (bitmask & 16) == 16)
            {
                page.RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message =
                                $"{dir.Resx.GetString("DIRECTIONS_NTF_SimRequired")}<br>{post}<br>{dir.Resx.GetString("DIRECTIONS_NTF_SimRequired1")}",
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false
                        }
                    });
            }
            else if (empl.SimRequired && (bitmask & 16) != 16)
            {
                if (empl.SimGprsPackage)
                    page.RenderNtf(w,
                        new List<Notification>
                        {
                            new Notification
                            {
                                Message =
                                    $"{dir.Resx.GetString("DIRECTIONS_NTF_SimRequired")}<br>{post}<br>{dir.Resx.GetString("DIRECTIONS_NTF_SimRequired2")}",
                                Status = NtfStatus.Error,
                                SizeIsNtf = false,
                                DashSpace = false
                            }
                        });

                else
                    page.RenderNtf(w,
                        new List<Notification>
                        {
                            new Notification
                            {
                                Message =
                                    $"{dir.Resx.GetString("DIRECTIONS_NTF_SimRequired")}<br>{post}<br>{dir.Resx.GetString("DIRECTIONS_NTF_SimRequired3")}",
                                Status = NtfStatus.Error,
                                SizeIsNtf = false,
                                DashSpace = false
                            }
                        });
            }
            else if (empl.SimRequired && (bitmask & 16) == 16)
            {
                if (empl.SimGprsPackage)
                    page.RenderNtf(w,
                        new List<Notification>
                        {
                            new Notification
                            {
                                Message =
                                    $"{dir.Resx.GetString("DIRECTIONS_NTF_SimRequired")}<br>{post}<br>{dir.Resx.GetString("DIRECTIONS_NTF_SimRequired2")}",
                                Status = (bitmask & 32) != 32 ? NtfStatus.Error : NtfStatus.Information,
                                SizeIsNtf = false,
                                DashSpace = false
                            }
                        });

                else
                    page.RenderNtf(w,
                        new List<Notification>
                        {
                            new Notification
                            {
                                Message =
                                    $"{dir.Resx.GetString("DIRECTIONS_NTF_SimRequired")}<br>{post}<br>{dir.Resx.GetString("DIRECTIONS_NTF_SimRequired3")}",
                                Status = (bitmask & 32) != 32 ? NtfStatus.Error : NtfStatus.Information,
                                SizeIsNtf = false,
                                DashSpace = false
                            }
                        });
            }
            else
            {
                w.Write("");
            }
        }

        private static void GetInconsistenciesString(TextWriter w, Direction dir, int cnt, string typeRules, int typeString = 0, bool inverse = false)
        {
           
            var li = new List<string>();
            if (inverse) li.Add(dir.Resx.GetString("DIRECTIONS_NTF_Inconsist_No"));

            switch (typeRules)
            {
                case "Роли":
                    if (typeString == 0)
                    {
                        if (cnt == 1)
                            li.Add(dir.Resx.GetString("DIRECTIONS_NTF_Inconsist_Роли_1"));
                        else
                            li.Add(dir.Resx.GetString("DIRECTIONS_NTF_Inconsist_Роли_2"));
                    }
                    else li.Add(dir.Resx.GetString("DIRECTIONS_NTF_Inconsist_Роли_3"));
                    break;
                case "Общие папки":
                    if (typeString == 0)
                    {
                        if (cnt == 1)
                            li.Add(dir.Resx.GetString("DIRECTIONS_NTF_Inconsist_Папки_1"));
                        else
                            li.Add(dir.Resx.GetString("DIRECTIONS_NTF_Inconsist_Папки_2"));
                    }
                    else
                    {
                        if (cnt == 1)
                            li.Add(dir.Resx.GetString("DIRECTIONS_NTF_Inconsist_Папки_3"));
                        else
                            li.Add(dir.Resx.GetString("DIRECTIONS_NTF_Inconsist_Папки_4"));
                    }
                    break;
                case "Доп. параметры":
                    if (typeString == 0)
                    {
                        if (cnt == 1)
                            li.Add(dir.Resx.GetString("DIRECTIONS_NTF_Inconsist_Параметры_1"));
                        else
                            li.Add(dir.Resx.GetString("DIRECTIONS_NTF_Inconsist_Параметры_2"));
                    }
                    break;
                case "Типы лиц":

                    if (typeString == 0)
                    {
                        if (cnt == 1)
                            li.Add(dir.Resx.GetString("DIRECTIONS_NTF_Inconsist_Типы_1"));
                        else
                            li.Add(dir.Resx.GetString("DIRECTIONS_NTF_Inconsist_Типы_2"));
                    }
                    else
                    {
                        if (cnt == 1)
                            li.Add(dir.Resx.GetString("DIRECTIONS_NTF_Inconsist_Типы_3"));
                        else
                            li.Add(dir.Resx.GetString("DIRECTIONS_NTF_Inconsist_Типы_4"));

                    }
                    break;
                default:
                    w.Write("#Ошибка определения вида прав!");
                    break;
            }

            w.Write(String.Join(" ", li.ToArray()));
        }

        /// <summary>
        ///     Вывод иформации о различиях в правах группы
        /// </summary>
        /// <param name="w">Поток вывода</param>
        /// <param name="typeRules">Тип права</param>
        /// <param name="advGrandGroup">Код группы доп.параметров</param>
        public static void CheckGroupInconsistencies(TextWriter w, Direction dir, string typeRules, int advGrandGroup = 0)
        {
            if (dir.IsNew) return;
            if (string.IsNullOrEmpty(dir.SotrudnikField.ValueString)) return;
            
            if (string.IsNullOrEmpty(dir.Sotrudnik.CommonEmployeeID) ||
                string.IsNullOrEmpty(dir.LoginField.ValueString))
                return;

            var dtI = dir.GetAccessGroupInconsistencies;
            var drs = dtI.AsEnumerable().Where(x =>
                x["ВидПрав"].ToString() == typeRules && (advGrandGroup == 0 ||
                                                         x["ПараметрОтноситсяК"].Equals(DBNull.Value) ||
                                                         int.Parse(x["ПараметрОтноситсяК"].ToString()) ==
                                                         advGrandGroup));

            if (!drs.Any()) return;

            foreach (var dr in drs)
            {
                w.Write($"<div class=\"v4Error v4ContextHelp\" title=\"{System.Web.HttpUtility.HtmlEncode(dir.Resx.GetString("DIRECTIONS_MSG_GroupRules_Title"))}\">");

                
                w.Write(dr["СотрудникиУКогоЕсть"]);
                w.Write(" ");
                GetInconsistenciesString(w, dir, int.Parse(dr["КоличествоЕсть"].ToString()), typeRules);
                w.Write(", ");
                w.Write(dr["СотрудникиУКогоНет"]);
                w.Write(" ");
                GetInconsistenciesString(w, dir, int.Parse(dr["КоличествоНет"].ToString()), typeRules, 0, true);
                w.Write(" ");
                GetInconsistenciesString(w, dir, int.Parse(dr["КоличествоНет"].ToString()), typeRules, 1);
                

                w.Write(" ");
                w.Write(dr["Отсутствует"]);
                w.Write("</div>");

            }

            
        }


        /// <summary>
        /// Проверка, что введенный телефон является мобильным
        /// </summary>
        /// <param name="page"></param>
        /// <param name="w"></param>
        /// <param name="dir"></param>
        /// <param name="phoneNumber"></param>
        public static void CheckPhoneNumberIsMobile(EntityPage page, TextWriter w, Direction dir, string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber)) return;

            phoneNumber = Regex.Replace(phoneNumber, "[^\\d]", "", RegexOptions.IgnoreCase);

            var sqlParams = new Dictionary<string, object>
            {
                {"@НомерТелефона", phoneNumber},
                
            };
            using (
                var dbReader = new DBReader(SQLQueries.SELECT_2356_CHECK_НомерТелефонаЯвляетсяМобильным, CommandType.Text,
                    Config.DS_user, sqlParams))
            {
                if (!dbReader.HasRows)
                    page.RenderNtf(w, new List<Notification> {
                    new Notification() {
                            Message = dir.Resx.GetString("DIRECTIONS_Msg_Err_МобТелефонТип"),
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = dir.Resx.GetString("DIRECTIONS_Msg_Err_МобТелефонТип_Title")
                    }
                });

            }


           
        }
    }
}
