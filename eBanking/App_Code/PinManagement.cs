using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using eBanking.Models;
using eBanking.App_Start;
using System.Data.Entity;

namespace eBanking.App_Code
{
    public static class PinManagement
    {
        private static eBankingDbContext db = new eBankingDbContext();
       
        //hear a is a string in which pin is genarate
        public static string GeneratePinCode()
        {

            string PinCode = AutoNumberGenarator("0123456789",11);

            return PinCode;
          
        }

        public static long GenerateSerialNumber(string PinPrefix)
        //{
        //    long serialNumber = ConstMessage.PIN_SERIAL_OFFSET + Id ;
        //    //string SerialNumber = AutoNumberGenarator("12345678908765432",11);
        //    return serialNumber.ToString();
        //}
        {
            Sequencer sequencer = new Sequencer();
            long pinSerial = 0;
            //_variable.AllStringVar = prefix + "-" + date;

            //check is this sequencer already exits if exists then update SqlNumber
            //or if not exists then create new one and return the SqlNumber

            try
            {

                var check = db.Sequencers.Where(x => x.Prefix == PinPrefix).SingleOrDefault();

                //already exists so Update the SqlNumber
                if (check != null)
                {
                    check.SqlNumber = check.SqlNumber + 1;
                    db.Entry(check).State = EntityState.Modified;
                    //_variable.AllStringVar = _variable.AllStringVar + "-" + check.SqlNumber;
                    pinSerial = check.SqlNumber;
                }

                    //does not exists so create new one
                else
                {
                    sequencer.SqlNumber = 1;
                    sequencer.Prefix = PinPrefix;
                    sequencer.PrefixType = ConstMessage.PrefixType_Pins; 
                    db.Sequencers.Add(sequencer);

                    pinSerial = sequencer.SqlNumber; //_variable.AllStringVar = _variable.AllStringVar + "-" + sequencer.SqlNumber;
                }

                db.SaveChanges();



            }
            catch (Exception ex)
            {
                string m = ex.Message;
                pinSerial = 0;
            }

            return pinSerial;
        }

        public static List<Sequencer> GetPinPrefixes()
        {
            try
            {
                return db.Sequencers.Where(p => p.PrefixType == ConstMessage.PrefixType_Pins).ToList(); 
            }
            catch (Exception) { }
            return null;
        }
        public static string GenerateBatchNumber()
        {

            string BatchNumber = AutoNumberGenarator("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890",11);
            BatchNumber = BatchNumber + DateTime.Now;
            return BatchNumber;
                 
        }

        public static string AutoNumberGenarator(string Element,int _maxSize)
        {
            int maxSize = _maxSize;

            //int maxSize = 16;
            //int minSize = 8 ;

            char[] chars = new char[62];
            string a;
            a = Element;
            //a = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            chars = a.ToCharArray();
            int size = maxSize;
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            size = maxSize;
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(size);

            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
                var code = result.ToString();
            }
                   

            return result.ToString();
        
        }
        public static bool HasPinCode(string genaratedPinCode)
        { 
           //To do check from db if pin already exists or not
           var code= DateTime.Now.ToString().GetHashCode().ToString("x"); 
            return true;
        }


        private static string Get4digitPin()
          {
              ////for 8 digit
              var bytes = new byte[4];
              var rng = RandomNumberGenerator.Create();
              rng.GetBytes(bytes);
              uint random = BitConverter.ToUInt32(bytes, 0) % 1000000000;
              return String.Format("{0:D8}", random);
          }

        public static string DoUniqueThePinCode(string PinCode, int Id)
        {
            //remove the first charecter from PinCode

            PinCode = PinCode.Remove(0, Id.ToString().Length);
            PinCode = Id + PinCode;

            return PinCode;
        }

      

        public static bool CheckPinStatus(int pinId)
        {
            try
            {
                var IsPinUsed = db.Transactions.Where(x => x.PinId == pinId).Select(x => x).ToList();

                if (IsPinUsed.Count() == 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            { 
               //to do log file
               
                string a = ex.Message;
            }

            return false;
        }
    
    }
}