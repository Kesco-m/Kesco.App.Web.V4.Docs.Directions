using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities;
using Kesco.Lib.Entities.Documents.EF.Directions;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Web.Settings;

namespace Kesco.App.Web.Docs.Directions
{
    public class ValidationMessages
    {
        private static readonly ResourceManager LocalResx =
            new ResourceManager("Kesco.App.Web.Docs.Directions.DirectionIT", Assembly.GetExecutingAssembly());

        /// <summary>
        ///     Проверка существования других указаний
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static void CheckExistsDirection(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.WorkPlaceField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }

            var sqlParams = new Dictionary<string, object>
            {
                {"@КодСотрудника", dir.SotrudnikField.Value},
                {"@КодРасположения", dir.WorkPlaceField.Value},
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

                    var list = new List<string> {LocalResx.GetString("_Msg_ExistsDirection") + ":"};
                    while (dbReader.Read())
                    {
                        var wr = new StringWriter();
                        page.RenderLinkDocument(wr, dbReader.GetInt32(colКодДокумента), false, NtfStatus.Information);
                        wr.Write(dbReader.GetString(colДокумент));
                        page.RenderLinkEnd(wr);

                        list.Add(wr.ToString());
                    }
                    page.RenderNtf(w, list, NtfStatus.Error);
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
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.Sotrudnik.Unavailable)
            {
                w.Write("");
                return;
            }

            var listEmpl = dir.Sotrudnik.EmployeesOnWorkPlace(dir.WorkPlaceField.ValueInt);

            if (listEmpl.Count > 0)
            {
                var wr = new StringWriter();
                if (!dir.LocationWorkPlace.Unavailable && dir.LocationWorkPlace.IsComputeredWorkPlace)
                    wr.Write(LocalResx.GetString("_NTF_ПосменнаяРабота") + ": ");
                else
                    wr.Write(LocalResx.GetString("_NTF_РабМестаВместе") + ": ");
                var inx = 1;
                foreach (var empl in listEmpl)
                {
                    page.RenderLinkEmployee(wr, "", empl, NtfStatus.Error);
                    if (inx < listEmpl.Count) wr.Write("; ");
                    inx++;
                }
                page.RenderNtf(w, new List<string> {wr.ToString()});
            }
            else
                w.Write("");
        }

        /// <summary>
        ///     Проверка, что указанное рабочее место есть среди рабочих мест сотрудника
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static void CheckSotrudnikWorkPlace(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.Sotrudnik.Unavailable)
            {
                w.Write("");
                return;
            }

            var workplaces = dir.Sotrudnik.Workplaces;
            if (workplaces != null && !workplaces.Any(x => x.Id.Equals(dir.LocationWorkPlace.Id)))
                page.RenderNtf(w, new List<string> {LocalResx.GetString("_Msg_РабочееМестоНеСоответствует")});
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
            if (dir.WorkPlaceField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }

            if (!dir.LocationWorkPlace.IsOffice)
                page.RenderNtf(w, new List<string> {LocalResx.GetString("_Msg_РабочееМестоНеВОфисе")});
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
            if (dir.WorkPlaceField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }

            if (!dir.LocationWorkPlace.IsComputeredWorkPlace && !dir.LocationWorkPlace.IsGuestWorkPlace)
                page.RenderNtf(w, new List<string> {LocalResx.GetString("_Msg_НеКомпьютеризированноеРабочееМесто")});
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
            if ((dir.MailNameField.ValueString.Length == 0 || dir.DomainField.ValueString.Length == 0))
            {
                w.Write("");
                return;
            }
            var email = dir.MailNameField.ValueString + "@" + dir.DomainField.ValueString;

            if (!Validation.IsEmail(email))
                w.Write(LocalResx.GetString("_Msg_EmailНеКорректен"));
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
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.Sotrudnik.Unavailable
                || (dir.MailNameField.ValueString.Length == 0 || dir.DomainField.ValueString.Length == 0))
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
                if (empl.Guid.ToString() != result.Properties["objectGUID"][i].ToString())
                    emailUsersGuid.Add(result.Properties["objectGUID"][i].ToString());
            }
            if (emailUsersGuid.Count == 0)
            {
                w.Write("");
                return;
            }

            var emailAddress = new StringCollection();
            for (var i = 0; i < result.Properties["proxyAddresses"].Count; i++)
            {
                if (result.Properties["proxyAddresses"][i].ToString()
                    .StartsWith("SMTP", StringComparison.CurrentCultureIgnoreCase))
                    emailAddress.Add(result.Properties["proxyAddresses"][i].ToString().Substring(5).Trim());
            }

            if (!emailAddress.Contains(email))
            {
                w.Write("");
                return;
            }

            w.Write(LocalResx.GetString("_NTF_EmailNotUnique"));
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
                w.Write(LocalResx.GetString("_Msg_ЛогинНеКорректен"));
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
                || (dir.LoginField.ValueString.Length == 0))
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
                    Config.DS_document, sqlParams))
            {
                if (dbReader.HasRows)
                    w.Write(LocalResx.GetString("_Msg_ЛогинСуществует"));
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
        public static void CheckSotrudnikParent(EntityPage page, TextWriter w, Direction dir, List<string> ntfList)
        {
            if (dir.SotrudnikParentField.ValueString.Length == 0 ||
                dir.SotrudnikParentCheckField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }
            var p = dir.SotrudnikParent;
            var bitMask = int.Parse(dir.SotrudnikParentCheckField.ValueString);


            if ((bitMask & 1) == 1 && (p.Login.Length == 0 || p.LoginFull.Length == 0))
                ntfList.Add(LocalResx.GetString("_Msg_СотрудникНеИмеетЛогина"));

            if ((bitMask & 2) == 2 && (p.Login.Length == 0 || p.LoginFull.Length == 0)
                && dir.SotrudnikField.ValueString.Length > 0 && !dir.Sotrudnik.Unavailable &&
                (dir.Sotrudnik.LoginFull.Length == 0 || dir.Sotrudnik.Login.Length == 0))
                ntfList.Add(LocalResx.GetString("_Msg_СотрудникНеИмеетЛогина"));

            if (dir.SotrudnikField.ValueString.Length > 0 &&
                dir.SotrudnikField.ValueString.Equals(dir.SotrudnikParentField.ValueString))
                ntfList.Add(LocalResx.GetString("_NTF_СотрудникСовпадает").ToLower());

            if (ntfList.Count > 0)
                page.RenderNtf(w, ntfList, NtfStatus.Error);
        }

        /// <summary>
        ///     Проверка состояния выбранного сотрудника-родителя
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        /// <param name="ntfList">Список предупреждений</param>
        public static void CheckSotrudnikParentStatus(EntityPage page, TextWriter w, Direction dir, List<string> ntfList)
        {
            if (dir.SotrudnikParentField.ValueString.Length == 0 ||
                dir.SotrudnikParentCheckField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }
            var p = dir.SotrudnikParent;
            var bitMask = int.Parse(dir.SotrudnikParentCheckField.ValueString);

            switch (p.Status)
            {
                case 0:
                    if ((bitMask & 2) == 2) ntfList.Add(LocalResx.GetString("_Msg_СотрудникНеУволен"));
                    break;
                case 1:
                    ntfList.Add(LocalResx.GetString("_Msg_СотрудникСостояние1"));
                    break;
                case 2:
                    ntfList.Add(LocalResx.GetString("_Msg_СотрудникСостояние2"));
                    break;
                case 3:
                    if ((bitMask & 1) == 1) ntfList.Add(LocalResx.GetString("_Msg_СотрудникСостояние3"));
                    break;
                case 4:
                    ntfList.Add(LocalResx.GetString("_Msg_СотрудникСостояние4"));
                    break;
                case 5:
                    ntfList.Add(LocalResx.GetString("_Msg_СотрудникСостояние5"));
                    break;
            }
        }

        /// <summary>
        ///     Проверка выбранного сотрудника
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        /// <param name="ntfList">Список предупреждений</param>
        public static void CheckSotrudnik(EntityPage page, TextWriter w, Direction dir, List<string> ntfList)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.Sotrudnik.Unavailable)
            {
                w.Write("");
                return;
            }
            
            if (dir.Sotrudnik.PersonEmployeeId == null)
                ntfList.Add(LocalResx.GetString("_Msg_СотрудникНетЛица"));

            if (dir.Sotrudnik.Workplaces.Count == 0)
                ntfList.Add(LocalResx.GetString("_Msg_СотрудникНетРабМеста"));

            if (dir.Sotrudnik.Photos.Count == 0)
                ntfList.Add(LocalResx.GetString("_Msg_ФотоНет"));

            if (ntfList.Count > 0)
                page.RenderNtf(w, ntfList, NtfStatus.Error);
        }

        /// <summary>
        ///     Проверка состояния выбранного сотрудника-родителя
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        /// <param name="ntfList">Список предупреждений</param>
        public static void CheckSotrudnikStatus(EntityPage page, TextWriter w, Direction dir, List<string> ntfList)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.SotrudnikField.ValueString.Length == 0 ||
                dir.Sotrudnik.Unavailable || dir.Sotrudnik.Status == 0)
            {
                w.Write("");
                return;
            }

            switch (dir.Sotrudnik.Status)
            {
                case 1:
                    ntfList.Add(LocalResx.GetString("_Msg_СотрудникСостояние1"));
                    break;
                case 2:
                    ntfList.Add(LocalResx.GetString("_Msg_СотрудникСостояние2"));
                    break;
                case 3:
                    ntfList.Add(LocalResx.GetString("_Msg_СотрудникСостояние3"));
                    break;
                case 4:
                    ntfList.Add(LocalResx.GetString("_Msg_СотрудникСостояние4"));
                    break;
                case 5:
                    ntfList.Add(LocalResx.GetString("_Msg_СотрудникСостояние5"));
                    break;
            }
        }

        /// <summary>
        /// Проверка, что на указанном рабочем месте работает группа сотруников
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static void CheckSotrudnikWorkPlaceGroup(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.ValueString.Length == 0 || dir.WorkPlaceField.ValueString.Length == 0)
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
                    w.Write("На указанном рабочем месте уже работает группа посменной работы " + dbReader.GetString(colСотрудник) + "!");
                    return;
                }

            }
            
        }

        /// <summary>
        /// Получение информации о необходимости выдачи Sim-карты
        /// </summary>
        /// <param name="page">Текущая страница</param>
        /// <param name="w">Поток вывода</param>
        /// <param name="dir">Текущий документ</param>
        public static void CheckSimInfo(EntityPage page, TextWriter w, Direction dir)
        {
            if (dir.SotrudnikField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }

            var empl = dir.Sotrudnik;

            var post = empl.SimPost.Length == 0 ? dir.SotrudnikPostField.ValueString : empl.SimPost;
            var bitmask = dir.PhoneEquipField.ValueString.Length > 0 ? dir.PhoneEquipField.ValueInt : 0;

            if (!empl.SimRequired && (bitmask & 16) == 16)
                page.RenderNtf(w, new List<string> { string.Format("сотруднику на должности<br>{0}<br>не требуется мобильная связь", post) }, NtfStatus.Error);
            else if (empl.SimRequired && (bitmask & 16) != 16)
            {
                if (empl.SimGprsPackage)
                    page.RenderNtf(w, new List<string> { string.Format("сотруднику на должности<br>{0}<br>требуется мобильная связь с заранее предоплаченным интернетом", post) }, NtfStatus.Error);
                else
                    page.RenderNtf(w, new List<string> { string.Format("сотруднику на должности<br>{0}<br>требуется мобильная связь с помегабайтной тарификацией", post) }, NtfStatus.Error);
            }
            else if (empl.SimRequired && (bitmask & 16) == 16)
            {
                if (empl.SimGprsPackage)
                    page.RenderNtf(w,
                        new List<string>
                        {
                            string.Format(
                                "сотруднику на должности<br>{0}<br>требуется мобильная связь с заранее предоплаченным интернетом",
                                post)
                        }, (bitmask & 32) != 32 ? NtfStatus.Error : NtfStatus.Information);
                else
                    page.RenderNtf(w,
                        new List<string>
                        {
                            string.Format(
                                "сотруднику на должности<br>{0}<br>требуется мобильная связь с помегабайтной тарификацией",
                                post)
                        }, (bitmask & 32) == 32 ? NtfStatus.Error : NtfStatus.Information);

            }
            else
            {
                w.Write("");
            }


        }
    }
}