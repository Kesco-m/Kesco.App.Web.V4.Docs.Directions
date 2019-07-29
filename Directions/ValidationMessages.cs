using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities;
using Kesco.Lib.Entities.Documents.EF.Directions;
using Kesco.Lib.Web.Controls.V4.Common;
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
        public static void CheckExistsDirection(EntityPage page, TextWriter w, Direction dir)
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
                {"@КодРасположения", dir.WorkPlaceField.Value},
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

                    var list = new List<Notification>
                    {
                        new Notification
                        {
                            Message = page.Resx.GetString("DIRECTIONS_Msg_ExistsDirection") + ":",
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = page.Resx.GetString("DIRECTIONS_Msg_ExistsDirection_Title")
                        }
                    };
                    while (dbReader.Read())
                    {
                        var wr = new StringWriter();
                        page.RenderLinkDocument(wr, dbReader.GetInt32(colКодДокумента), false, NtfStatus.Error);
                        wr.Write(dbReader.GetString(colДокумент));
                        page.RenderLinkEnd(wr);

                        list.Add(new Notification
                        {
                            Message = wr.ToString(),
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = page.Resx.GetString("DIRECTIONS_Msg_ExistsDirection_Title")
                        });
                    }

                    page.RenderNtf(w, list);
                }
                else
                {
                    w.Write("");
                }
            }
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

            var listEmpl = dir.Sotrudnik.EmployeesOnWorkPlace(dir.WorkPlaceField.ValueInt);

            if (listEmpl.Count > 0)
            {
                var wr = new StringWriter();
                if (!dir.LocationWorkPlace.Unavailable && dir.LocationWorkPlace.IsComputeredWorkPlace)
                    wr.Write(page.Resx.GetString("DIRECTIONS_NTF_ПосменнаяРабота") + ": ");
                else
                    wr.Write(page.Resx.GetString("DIRECTIONS_NTF_РабМестаВместе") + ": ");
                var inx = 1;
                foreach (var empl in listEmpl)
                {
                    page.RenderLinkEmployee(wr, "", empl, NtfStatus.Error, false);
                    if (inx < listEmpl.Count) wr.Write("; ");
                    inx++;
                }

                page.RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message = wr.ToString(),
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = page.Resx.GetString("DIRECTIONS_NTF_ПосменнаяРабота_Title")
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
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.Sotrudnik.Unavailable || dir.Finished)
            {
                w.Write("");
                return;
            }

            var workplaces = dir.Sotrudnik.Workplaces;
            if (workplaces != null && !workplaces.Any(x => x.Id.Equals(dir.LocationWorkPlace.Id)))
                page.RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message = page.Resx.GetString("DIRECTIONS_Msg_РабочееМестоНеСоответствует"),
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = page.Resx.GetString("DIRECTIONS_Msg_РабочееМестоНеСоответствует_Title")
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
        public static void CheckExistsWorkPlaceIsComputeredNoOrganized(EntityPage page, TextWriter w, Direction dir,
            int wpId = 0)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.Sotrudnik.Unavailable || dir.Finished)
            {
                w.Write("");
                return;
            }

            if (dir.Sotrudnik.Workplaces.Any(x => x.IsComputeredWorkPlace && !x.IsOrganized && int.Parse(x.Id) != wpId))
                page.RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message = page.Resx.GetString("DIRECTIONS_Msg_УСотрудникаЕстьРабМесто"),
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = page.Resx.GetString("DIRECTIONS_Msg_УСотрудникаЕстьРабМесто_Title")
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
            if (dir.WorkPlaceField.ValueString.Length == 0 || dir.Finished)
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
                            Message = page.Resx.GetString("DIRECTIONS_Msg_РабочееМестоНеВОфисе"),
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = page.Resx.GetString("DIRECTIONS_Msg_РабочееМестоНеВОфисе_Title")
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
            if (dir.WorkPlaceField.ValueString.Length == 0 || dir.Finished)
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
                            Message = page.Resx.GetString("DIRECTIONS_Msg_НеКомпьютеризированноеРабочееМесто"),
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = page.Resx.GetString("DIRECTIONS_Msg_НеКомпьютеризированноеРабочееМесто_Title")
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
                w.Write(page.Resx.GetString("DIRECTIONS_Msg_EmailНеКорректен"));
            else
                w.Write("");
        }

        /// <summary>
        ///     Проверка введенного Email на уникальность
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static void CheckUniqueEmail(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.Sotrudnik.Unavailable ||
                dir.MailNameField.ValueString.Length == 0 || dir.DomainField.ValueString.Length == 0 || dir.Finished)
            {
                w.Write("");
                return;
            }

            var email = dir.MailNameField.ValueString + "@" + dir.DomainField.ValueString;
            var result = ADSI.FindAccountByEmail(email);
            if (result == null)
            {
                w.Write("");
                return;
            }

            var empl = dir.Sotrudnik;
            var emailUsersGuid = new StringCollection();
            for (var i = 0; i < result.Properties["objectGUID"].Count; i++)
            {
                var adsiGuid = new Guid((byte[]) result.Properties["objectGUID"][i]);
                if (!empl.Guid.Equals(adsiGuid))
                    emailUsersGuid.Add(adsiGuid.ToString());
            }

            if (emailUsersGuid.Count == 0)
            {
                w.Write("");
                return;
            }

            var emailAddress = new StringCollection();
            for (var i = 0; i < result.Properties["proxyAddresses"].Count; i++)
                if (result.Properties["proxyAddresses"][i].ToString()
                    .StartsWith("SMTP", StringComparison.CurrentCultureIgnoreCase))
                    emailAddress.Add(result.Properties["proxyAddresses"][i].ToString().Substring(5).Trim());

            if (!emailAddress.Contains(email))
            {
                w.Write("");
                return;
            }

            w.Write(page.Resx.GetString("DIRECTIONS_NTF_EmailNotUnique"));
            w.Write("<br/>");
            w.Write(page.Resx.GetString("DIRECTIONS_NTF_EmailNotUnique1"));
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
                                                           || dir.MailNameField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }

            if (!Validation.IsNameInEmail(dir.MailNameField.ValueString, dir.LoginField.ValueString,
                dir.Sotrudnik.FirstNameEn, dir.Sotrudnik.LastNameEn))
                w.Write(page.Resx.GetString("DIRECTIONS_NTF_EmailName"));
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


            var dm = dir.DomainNames.FirstOrDefault(x => x.Id == dir.DomainField.ValueString);
            if (dm == null)
            {
                w.Write("");
                return;
            }

            var pIds = Convert.Str2Collection(dm.PersonIds);
            if (pIds.Count == 0)
            {
                w.Write("");
                return;
            }

            foreach (var p in pIds)
                if (int.Parse(p) == dir.Sotrudnik.PersonEmployeeId || int.Parse(p) == dir.Sotrudnik.OrganizationId)
                {
                    w.Write("");
                    return;
                }

            w.Write(page.Resx.GetString("DIRECTIONS_NTF_EmailDomain"));
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
                w.Write(page.Resx.GetString("DIRECTIONS_Msg_ЛогинНеКорректен"));
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
                                                           || dir.LoginField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }

            if (!Validation.IsNameInLogin(dir.LoginField.ValueString, dir.Sotrudnik.LastNameEn))
                w.Write(page.Resx.GetString("DIRECTIONS_Msg_ЛогинИмя"));
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
                    w.Write(page.Resx.GetString("DIRECTIONS_Msg_ЛогинСуществует"));
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


            if ((bitMask & 1) == 1 && p.Login.Length == 0 && !dir.Finished)
                ntfList.Add(new Notification
                {
                    Message = page.Resx.GetString("DIRECTIONS_Msg_СотрудникНеИмеетЛогина"),
                    Status = NtfStatus.Error,
                    SizeIsNtf = false,
                    DashSpace = false,
                    Description = page.Resx.GetString("DIRECTIONS_Msg_СотрудникНеИмеетЛогина_Title")
                });

            if ((bitMask & 2) == 2
                && p.Login.Length == 0
                && dir.SotrudnikField.ValueString.Length > 0
                && !dir.Sotrudnik.Unavailable
                && dir.Sotrudnik.Login.Length == 0
                && !dir.Finished)
                ntfList.Add(new Notification
                {
                    Message = page.Resx.GetString("DIRECTIONS_Msg_СотрудникНеИмеетЛогина"),
                    Status = NtfStatus.Error,
                    SizeIsNtf = false,
                    DashSpace = false,
                    Description = page.Resx.GetString("DIRECTIONS_Msg_СотрудникНеИмеетЛогина_Title")
                });

            if (dir.SotrudnikField.ValueString.Length > 0 &&
                dir.SotrudnikField.ValueString.Equals(dir.SotrudnikParentField.ValueString))
                ntfList.Add(new Notification
                {
                    Message = page.Resx.GetString("DIRECTIONS_NTF_СотрудникСовпадает")?.ToLower(),
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
            var description = page.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние_Title");
            switch (p.Status)
            {
                case 0:
                    if ((bitMask & 2) == 2)
                    {
                        message = page.Resx.GetString("DIRECTIONS_Msg_СотрудникНеУволен");
                        description = page.Resx.GetString("DIRECTIONS_Msg_СотрудникНеУволен_Title");
                    }

                    break;
                case 1:
                    if ((bitMask & 2) != 2)
                        message = page.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние1");
                    break;
                //case 2:
                //    message = page.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние2");
                //    break;
                case 3:
                    if ((bitMask & 1) == 1)
                    {
                        message = page.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние3");
                        description = page.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние3_Title");
                    }

                    break;
                case 4:
                    message = page.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние4");
                    break;
                case 5:
                    message = page.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние5");
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

            if (dir.Sotrudnik.PersonEmployeeId == null && dir.Sotrudnik.GroupMembers.Count == 0
            ) //!!! Не является общим сотрудником ДОБАВИТЬ проверку
                ntfList.Add(new Notification
                {
                    Message = page.Resx.GetString("DIRECTIONS_Msg_СотрудникНетЛица"),
                    Status = NtfStatus.Error,
                    SizeIsNtf = false,
                    DashSpace = false,
                    Description = page.Resx.GetString("DIRECTIONS_Msg_СотрудникНетЛица_Title")
                });

            //if (dir.Sotrudnik.Photos.Count == 0 && dir.Sotrudnik.GroupMembers.Count == 0)
            //    ntfList.Add(new Notification
            //    {
            //        Message = page.Resx.GetString("DIRECTIONS_Msg_ФотоНет"),
            //        Status = NtfStatus.Error,
            //        SizeIsNtf = false,
            //        DashSpace = false,
            //        Description = page.Resx.GetString("DIRECTIONS_Msg_ФотоНет_Title")
            //    });

            if (ntfList.Count > 0 && render)
                page.RenderNtf(w, ntfList);
        }

        /// <summary>
        ///     Проверка наличия рабочего места у выбранного сотрудника
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        /// <param name="ntfList">Список предупреждений</param>
        /// <param name="render">Вывести в поток</param>
        public static void CheckSotrudnikWorkPlace(EntityPage page, TextWriter w, Direction dir,
            List<Notification> ntfList, bool render = true)
        {
            if (!dir.Finished && dir.Sotrudnik.Workplaces.Count == 0 && (dir.WorkPlaceTypeField.ValueInt & 1) == 1)
            {
                ntfList.Add(new Notification
                {
                    Message = page.Resx.GetString("DIRECTIONS_Msg_РабочееМестоНеСоответствует"),
                    Status = NtfStatus.Empty,
                    SizeIsNtf = false,
                    DashSpace = false,
                    Description = page.Resx.GetString("DIRECTIONS_Msg_РабочееМестоНеСоответствует_Title")
                });

                if (render)
                    page.RenderNtf(w, ntfList);
            }
        }

        /// <summary>
        ///     Проверка состояния выбранного сотрудника-родителя
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        /// <param name="ntfList">Список предупреждений</param>
        public static void CheckSotrudnikStatus(EntityPage page, TextWriter w, Direction dir,
            List<Notification> ntfList)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.SotrudnikField.ValueString.Length == 0 ||
                dir.Sotrudnik.Unavailable || dir.Sotrudnik.Status > 2 || dir.Finished)
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
                    message = page.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние3");
                    break;
                case 4:
                    message = page.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние4");
                    break;
                case 5:
                    message = page.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние5");
                    break;
            }

            ntfList.Add(new Notification
            {
                Message = message,
                Status = NtfStatus.Error,
                DashSpace = false,
                SizeIsNtf = false,
                Description = page.Resx.GetString("DIRECTIONS_Msg_СотрудникСостояние_Title")
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
                        $"{page.Resx.GetString("DIRECTIONS_NTF_ГруппаНаРасположении")} {dbReader.GetString(colСотрудник)}!");
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

            var post = empl.SimPost.Length == 0 ? dir.SotrudnikPostField.ValueString : empl.SimPost;
            var bitmask = dir.PhoneEquipField.ValueString.Length > 0 ? dir.PhoneEquipField.ValueInt : 0;

            if (!empl.SimRequired && (bitmask & 16) == 16)
            {
                page.RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message =
                                $"{page.Resx.GetString("DIRECTIONS_NTF_SimRequired")}<br>{post}<br>{page.Resx.GetString("DIRECTIONS_NTF_SimRequired1")}",
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
                                    $"{page.Resx.GetString("DIRECTIONS_NTF_SimRequired")}<br>{post}<br>{page.Resx.GetString("DIRECTIONS_NTF_SimRequired2")}",
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
                                    $"{page.Resx.GetString("DIRECTIONS_NTF_SimRequired")}<br>{post}<br>{page.Resx.GetString("DIRECTIONS_NTF_SimRequired3")}",
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
                                    $"{page.Resx.GetString("DIRECTIONS_NTF_SimRequired")}<br>{post}<br>{page.Resx.GetString("DIRECTIONS_NTF_SimRequired2")}",
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
                                    $"{page.Resx.GetString("DIRECTIONS_NTF_SimRequired")}<br>{post}<br>{page.Resx.GetString("DIRECTIONS_NTF_SimRequired3")}",
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
    }
}