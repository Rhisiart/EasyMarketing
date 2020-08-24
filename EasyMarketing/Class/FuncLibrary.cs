using CrmEarlyBound;
using EasyMarketing.Api;
using EasyMarketing.Class.Interests;
using EasyMarketing.Class.Members;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EasyMarketing.Class
{
    public class FuncLibrary : IDisposable
    {
        public int CalculateAgeCorrect(DateTime birthDate, DateTime now)
        {
            int age = now.Year - birthDate.Year;

            if (now.Month < birthDate.Month || (now.Month == birthDate.Month && now.Day < birthDate.Day))
                age--;

            return age;
        }

        public string GetMailMd(string mailOriginal)
        {
            string mail = null;
            using (MD5 md5hash = MD5.Create())
            {
               mail = GetMdHash(md5hash, mailOriginal);
               if (VerifyMdHash(md5hash, mailOriginal, mail))
               {

               }
               else
               {

               }
            }
            return mail;
        }

        public string GetMdHash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public bool VerifyMdHash(MD5 md5Hash, string input, string hash)
        {
            string hashOfInput = GetMdHash(md5Hash, input);

            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public find_marketingautomationintegration GetMailchimpInfo(IOrganizationService service)
        { 
            QueryExpression QEfind_mailchimpintegration = new QueryExpression("find_marketingautomationintegration");
            QEfind_mailchimpintegration.ColumnSet.AddColumns("find_mailchimpintegration", "find_mailchimplistid", "find_mailaccount", "find_platform", "find_sendgridintegration");
            return service.RetrieveMultiple(QEfind_mailchimpintegration).Entities[0].ToEntity<find_marketingautomationintegration>();
        }

        public void Dispose()
        {
            
        }
    }
}
