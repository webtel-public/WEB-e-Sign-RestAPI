using iTextSharp.text;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;

namespace WEB_e_Sign_RestAPI.Controllers
{
    public class eSignController : ApiController
    {
        [HttpPost]
        [BasicAuthentication]
        [Route("SignPDF_Base64String")]
        public HttpResponseMessage Post([FromBody]Req_List obj_eSign)
        {
            HttpResponseMessage msg = new HttpResponseMessage();
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj_eSign);
            
            string pdfByte1 = obj_eSign.pdfByte1;
            String AuthorizedSignatory = obj_eSign.AuthorizedSignatory;
            String SignerName = obj_eSign.SignerName;
            int TopLeft = obj_eSign.TopLeft;
            int BottomLeft = obj_eSign.TopLeft;
            int TopRight = obj_eSign.TopLeft;
            int BottomRight = obj_eSign.TopLeft;
            String ExcludePageNo = obj_eSign.ExcludePageNo;
            string InvoiceNumber = obj_eSign.InvoiceNumber;
            int pageNo = obj_eSign.pageNo;
            string PrintDateTime = obj_eSign.PrintDateTime;
            string FindAuth = obj_eSign.FindAuth;
            int FindAuthLocation = obj_eSign.FindAuthLocation;
            int fontsize = obj_eSign.fontsize;
            if (fontsize == 0)
                fontsize = 24;
            
            int adjustCoordinates = obj_eSign.adjustCoordinates;
            int signOnlySearchTextPage = obj_eSign.signOnlySearchTextPage;

            string data1 = "";
            
            byte[] pdfByte = null;
            
            
            try
            {
                pdfByte = Convert.FromBase64String(pdfByte1);
            }
            catch { }

            string fname = Guid.NewGuid().ToString().Replace("-", "") + ".pdf";
            string fname1 = Guid.NewGuid().ToString().Replace("-", "") + ".png";
            try
            {
                if (!System.IO.Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "temp"))
                    System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "temp");
                File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath("~/temp/" + fname), pdfByte);

            }
            catch (Exception ex)
            {
                
                data1 = "{\"file\" : \"" + Convert.ToBase64String(pdfByte) + "\",\"status\" : \"failed\"," + Environment.NewLine + "\"error\" : \"" + "Invalid PDF : " + ex.Message + "\"" + "}";
                msg = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                msg.Content = new StringContent(data1, Encoding.UTF8, "application/json");
                return msg;

            }
            
            string outputPdf1 = AppDomain.CurrentDomain.BaseDirectory + "tempgen";

            if (!System.IO.Directory.Exists(outputPdf1))
                System.IO.Directory.CreateDirectory(outputPdf1);
            Cert cert = null;
            string ss = "";
            int Finali = -1;
            
            try
            {
                
                string pw = System.Configuration.ConfigurationManager.AppSettings["PWD"].ToString();
                
                cert = new Cert(AppDomain.CurrentDomain.BaseDirectory + "\\Cer.pfx", pw);

                string SubDN = cert.Chain[0].SubjectDN.GetValueList()[1].ToString();
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                
                string PrintDN = "";

                if (PrintDateTime.Trim().Length > 0)
                {
                    PrintDN = Environment.NewLine + Environment.NewLine + "Digitally Signed By " + SignerName + Environment.NewLine + "For : " + SubDN + Environment.NewLine + Environment.NewLine + "Date : " + PrintDateTime + " +" + timeZone.BaseUtcOffset.ToString().Substring(0, 5);
                }
                else
                {
                    string s1 = TimeZoneInfo.ConvertTime(DateTime.Now, timeZone).ToString("HH:mm:ss");

                    PrintDN = Environment.NewLine + "Digitally Signed By " + SignerName + Environment.NewLine + "For : " + SubDN + Environment.NewLine + "Date : " + DateTime.Now.ToString("yyyy.MM.dd") + "  " + s1 + " +" + timeZone.BaseUtcOffset.ToString().Substring(0, 5);
                }

                PdfReader pdfReader = new PdfReader(System.Web.Hosting.HostingEnvironment.MapPath("~/temp/" + fname));
                int TotalPage = pdfReader.NumberOfPages;
                if (TotalPage == 1)
                    pageNo = 1;
                if (pageNo == -2)
                {
                    pageNo = TotalPage;
                }
                DateTime dt = new DateTime();
                if (pageNo == -1)
                {
                    File.Copy(System.Web.Hosting.HostingEnvironment.MapPath("~/temp/" + fname), outputPdf1 + "\\" + Path.GetFileNameWithoutExtension(fname) + "0".ToString() + Path.GetExtension(fname));

                    PdfStamper pdfStamper = null;
                    PdfSignatureAppearance sap = null;
                    FileStream outputStream = null;
                    IExternalSignature es = null;
                    for (int i = 0; i < TotalPage; i++)
                    {
                        String[] exc = ExcludePageNo.Split(',');
                        bool exclu = false;
                        foreach (string exc1 in exc)
                        {
                            if (exc1.Trim().Length > 0)
                            {
                                if (Convert.ToInt32(exc1) == i + 1)
                                {
                                    exclu = true;
                                    break;
                                }
                            }
                        }
                        if (!exclu)
                        {
                            pdfReader = new PdfReader(outputPdf1 + "\\" + Path.GetFileNameWithoutExtension(fname) + i.ToString() + Path.GetExtension(fname));
                            outputStream = new FileStream(outputPdf1 + "\\" + Path.GetFileNameWithoutExtension(fname) + (i + 1).ToString() + Path.GetExtension(fname), FileMode.Create);
                            pdfStamper = PdfStamper.CreateSignature(pdfReader, outputStream, '\0', Path.GetTempFileName(), true);
                            pdfStamper.SetFullCompression();
                            sap = pdfStamper.SignatureAppearance;
                            sap.Acro6Layers = false;
                            sap.Image = null;
                            sap.SetVisibleSignature(new iTextSharp.text.Rectangle(TopLeft, BottomLeft, TopRight, BottomRight), i + 1, null);
                            sap.SignDate = dt;
                            sap.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.DESCRIPTION;
                            sap.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.GRAPHIC;
                            es = new PrivateKeySignature(cert.Akp, "SHA-256");
                            MakeSignature.SignDetached(sap, es, cert.Chain, null, null, null, 0, CryptoStandard.CMS);
                            File.Delete(outputPdf1 + "\\" + Path.GetFileNameWithoutExtension(fname) + i.ToString() + Path.GetExtension(fname));
                            Finali = i + 1;
                        }
                        else
                        {
                            Finali = i + 1;
                            File.Copy(outputPdf1 + "\\" + Path.GetFileNameWithoutExtension(fname) + i.ToString() + Path.GetExtension(fname), outputPdf1 + "\\" + Path.GetFileNameWithoutExtension(fname) + (i + 1).ToString() + Path.GetExtension(fname));

                            File.Delete(outputPdf1 + "\\" + Path.GetFileNameWithoutExtension(fname) + i.ToString() + Path.GetExtension(fname));
                        }
                    }
                    if (pdfStamper != null)
                    {
                        pdfStamper.Close();
                        pdfStamper.Dispose();
                        pdfStamper = null;
                    }
                    sap = null;
                    if (outputStream != null)
                    {
                        outputStream.Close();
                        outputStream.Dispose();
                        outputStream = null;
                    }
                    if (pdfReader != null)
                    {
                        pdfReader.Close();
                        pdfReader.Dispose();
                        pdfReader = null;
                    }

                    es = null;
                    cert = null;
                    sap = null;

                }
                else
                {

                    FileStream outputStream = null;
                    outputStream = new FileStream(outputPdf1 + "\\" + fname, FileMode.Create);
                    PdfStamper pdfStamper = null;
                    pdfStamper = PdfStamper.CreateSignature(pdfReader, outputStream, '1', Path.GetTempFileName(), true);
                    pdfStamper.SetFullCompression();
                    PdfSignatureAppearance sap = pdfStamper.SignatureAppearance;
                    sap.Acro6Layers = false;
                    sap.Image = null;

                    sap.SetVisibleSignature(new iTextSharp.text.Rectangle(TopLeft, BottomLeft, TopRight, BottomRight), pageNo, null);
                    sap.SignDate = dt;
                    
                    
                    IExternalSignature es = new PrivateKeySignature(cert.Akp, "SHA-256");
                    MakeSignature.SignDetached(sap, es, cert.Chain, null, null, null, 0, CryptoStandard.CMS);


                    pdfStamper.Close();
                    pdfStamper.Dispose();
                    pdfStamper = null;
                    sap = null;

                    outputStream.Close();
                    outputStream.Dispose();
                    outputStream = null;

                    pdfReader.Close();
                    pdfReader.Dispose();
                    pdfReader = null;

                    es = null;
                    cert = null;
                    sap = null;
                    
                }
            }
            catch (Exception ex)
            {
                
                data1 = "{\"file\" : \"" + Convert.ToBase64String(pdfByte) + "\",\"status\" : \"failed\"," + Environment.NewLine + "\"error\" : \"" + "Signing Failed : "+ex.Message+"\"" + "}";
                msg = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                msg.Content = new StringContent(data1, Encoding.UTF8, "application/json");

                
                try
                {
                    if (File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/tempgen/" + (Finali == -1 ? fname : Path.GetFileNameWithoutExtension(fname) + Finali.ToString() + Path.GetExtension(fname)))))
                        File.Delete(System.Web.Hosting.HostingEnvironment.MapPath("~/tempgen/" + (Finali == -1 ? fname : Path.GetFileNameWithoutExtension(fname) + Finali.ToString() + Path.GetExtension(fname))));
                }
                catch
                {
                    System.Threading.Thread tt = new System.Threading.Thread(DelFile);
                    tt.Start(System.Web.Hosting.HostingEnvironment.MapPath("~/tempgen/" + (Finali == -1 ? fname : Path.GetFileNameWithoutExtension(fname) + Finali.ToString() + Path.GetExtension(fname))));
                }
                try
                {
                    if (File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/temp/" + fname1)))
                        System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath("~/temp/" + fname1));
                }
                catch
                {
                    System.Threading.Thread tt = new System.Threading.Thread(DelFile);
                    tt.Start(System.Web.Hosting.HostingEnvironment.MapPath("~/tempgen/" + fname1));
                }
                try
                {
                    if (File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/tempgen/" + fname)))
                        System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath("~/tempgen/" + fname));
                }
                catch
                {
                    System.Threading.Thread tt = new System.Threading.Thread(DelFile);
                    tt.Start(System.Web.Hosting.HostingEnvironment.MapPath("~/tempgen/" + fname));
                }
                try
                {
                    if (File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/tempsig/" + Path.GetFileName(ss))))
                        System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath("~/tempsig/" + Path.GetFileName(ss)));
                    
                }
                catch (Exception e)
                {

                    
                    System.Threading.Thread tt = new System.Threading.Thread(DelFile);
                    tt.Start(ss);

                }
                return msg;


               
            }
            try
            {
                FileInfo fi = new FileInfo(System.Web.Hosting.HostingEnvironment.MapPath("~/tempgen/" + (Finali == -1 ? fname : Path.GetFileNameWithoutExtension(fname) + Finali.ToString() + Path.GetExtension(fname))));
                BinaryReader br = new BinaryReader(fi.OpenRead());
                
                data1 = "{\"file\" : \"" + Convert.ToBase64String(br.ReadBytes((int)fi.Length)) + "\",\"status\" : \"success\"," + Environment.NewLine + "\"error\" : \"\"" + "}";
                msg = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                msg.Content = new StringContent(data1, Encoding.UTF8, "application/json");
                fi = null;
                br.Close();
                br.Dispose();
                br = null;

            }
            catch (Exception ex)
            {
                data1 = "{\"file\" : \"" + Convert.ToBase64String(pdfByte) + "\",\"status\" : \"failed\"," + Environment.NewLine + "\"error\" : \"" + "Error on output generation : " + ex.Message + "\"" + "}";
                msg = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                msg.Content = new StringContent(data1, Encoding.UTF8, "application/json");

            }

            try
            {
                if (File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/temp/" + fname)))
                    System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath("~/temp/" + fname));
            }
            catch
            {
                System.Threading.Thread tt = new System.Threading.Thread(DelFile);
                tt.Start(System.Web.Hosting.HostingEnvironment.MapPath("~/temp/" + fname));
            }
            try
            {
                if (File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/tempgen/" + (Finali == -1 ? fname : Path.GetFileNameWithoutExtension(fname) + Finali.ToString() + Path.GetExtension(fname)))))
                    File.Delete(System.Web.Hosting.HostingEnvironment.MapPath("~/tempgen/" + (Finali == -1 ? fname : Path.GetFileNameWithoutExtension(fname) + Finali.ToString() + Path.GetExtension(fname))));
            }
            catch
            {
                System.Threading.Thread tt = new System.Threading.Thread(DelFile);
                tt.Start(System.Web.Hosting.HostingEnvironment.MapPath("~/tempgen/" + (Finali == -1 ? fname : Path.GetFileNameWithoutExtension(fname) + Finali.ToString() + Path.GetExtension(fname))));
            }
            try
            {
                if (File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/temp/" + fname1)))
                System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath("~/temp/" + fname1));
            }
            catch
            {
                System.Threading.Thread tt = new System.Threading.Thread(DelFile);
                tt.Start(System.Web.Hosting.HostingEnvironment.MapPath("~/tempgen/" + fname1));
            }
            try
            {
                if (File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/tempgen/" + fname)))
                System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath("~/tempgen/" + fname));
            }
            catch
            {
                System.Threading.Thread tt = new System.Threading.Thread(DelFile);
                tt.Start(System.Web.Hosting.HostingEnvironment.MapPath("~/tempgen/" + fname));
            }
            try
            {
                
                if (File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/tempsig/" + Path.GetFileName(ss))))
                System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath("~/tempsig/" + Path.GetFileName(ss)));
                

            }
            catch (Exception e)
            {
                
                System.Threading.Thread tt = new System.Threading.Thread(DelFile);
                tt.Start(ss);

            }
            
            GC.Collect();
            return msg;
            
        }
        
        private void DelFile(object fname)
        {
            try
            {
                System.Threading.Thread.Sleep(1500);
                System.IO.File.Delete((string)fname);
            }
            catch { }

        }

    }
}