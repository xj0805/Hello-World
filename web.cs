using System;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace WebService1
{
    /// <summary>
    /// Interface 的摘要说明
    /// </summary>
    public class Interface : IHttpHandler
    {
                public static SqlConnection sqlCon1;  //用于连接数据库  

        //将下面的引号之间的内容换成上面记录下的属性中的连接字符串  
        private String ConServerStr = @"Data Source=localhost;Initial Catalog=StockManage;Integrated Security=True";

        public MySoapHeader myHeader = new MySoapHeader();

        //默认构造函数  
        public Interface()
        {
            if (sqlCon1 == null)
            {
                sqlCon1 = new SqlConnection();
                sqlCon1.ConnectionString = ConServerStr;
                sqlCon1.Open();
            }
        }

        //关闭/销毁函数，相当于Close()  
        public void Dispose()
        {
            if (sqlCon1 != null)
            {
                sqlCon1.Close();
                sqlCon1 = null;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("Hello World");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

		        /// <summary>  
        /// PC上传工单列表
        /// </summary>  
        /// <returns>返回成功信息</returns>
        public string PCuploadWorkList(string xmlstring)
        {
            string workId = "";
            string orderType = "";
            string siteName = "";
            string datetime="";

            datetime = DateTime.Now.ToString();
            FileStream fs = new FileStream("C:\\uploadworklist.txt", FileMode.Append, FileAccess.Write);
            StreamWriter sr = new StreamWriter(fs);
            sr.WriteLine(datetime + xmlstring);//开始写入值
            sr.Close();
            fs.Close();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlstring.Trim());
                XmlNodeList xnl = doc.GetElementsByTagName("work");
                foreach (XmlNode node in xnl)
                {
                    XmlElement xe = (XmlElement)node;
                    workId = xe.SelectSingleNode("workId").InnerText.Trim();
                    string workerName = workId.Substring(workId.IndexOf("*")+1);
                    orderType = xe.SelectSingleNode("orderType").InnerText.Trim();
                    siteName = xe.SelectSingleNode("siteName").InnerText.Trim();
                    SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                    sqlCon.ConnectionString = ConServerStr;
                    sqlCon.Open();
                    string sql = "insert into WorkList (workId,orderType,siteName,workerName)"
                    + "values ('" + workId + "','" + orderType + "','" + siteName + "','" + workerName + "')";
                    SqlCommand cmd = new SqlCommand(sql, sqlCon);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    sqlCon.Dispose();//释放数据库连接资源
                }
            }
            catch (Exception e)
            {

                //return (Convert.ToString(e));
				return "false";

            }
            return "true";
        }
		
		
		       /// <summary>
        /// 6.5下载工单列表
        /// </summary>
        /// <param name="WorkList"></param>
        /// <returns></returns>
        public string downloadWorkList(String a)
        {
            string workerName = a;
            string datetime = "";
            datetime = DateTime.Now.ToString();
            XmlDocument doc = new XmlDocument();   //创建xml文件
            XmlDeclaration xdecl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);  //文件声明
            doc.AppendChild(xdecl);
            XmlElement xelen = doc.CreateElement("response");    //创建xml文件根节点
            doc.AppendChild(xelen);
            XmlElement xelen1 = doc.CreateElement("workList");   //创建下级子节点
            xelen.AppendChild(xelen1);
            try
            {
                SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon.ConnectionString = ConServerStr;
                sqlCon.Open();
                string sql = "select * from WorkList where workerName = '" + workerName + "'";//注意调用的是指定用户名
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                cmd.ExecuteNonQuery();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    XmlElement xel = doc.CreateElement("work");
                    xelen1.AppendChild(xel);
                    XmlElement Sub1 = doc.CreateElement("workId");
                    Sub1.InnerText = reader["workId"].ToString();
                    xel.AppendChild(Sub1);
                    XmlElement Sub2 = doc.CreateElement("orderType");
                    Sub2.InnerText = reader["orderType"].ToString();
                    xel.AppendChild(Sub2);
                    XmlElement Sub3 = doc.CreateElement("siteName");
                    Sub3.InnerText = reader["siteName"].ToString();
                    xel.AppendChild(Sub3);
                }
                reader.Close();                
                cmd.Dispose();
                sqlCon.Dispose();//释放数据库连接资源

                string sql1 = "delete from WorkList where workerName='" + workerName + "'";
                SqlCommand sqlcmd1 = new SqlCommand(sql1, sqlCon1);
                sqlcmd1.ExecuteNonQuery();
                sqlcmd1.Dispose();
            }
            catch (Exception e)
            {
                FileStream fs1 = new FileStream("C:\\iODHerror.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sr1 = new StreamWriter(fs1);
                sr1.WriteLine(datetime + " downloadworklist " + Convert.ToString(e));//开始写入值
                sr1.Close();
                fs1.Close();
//                XmlElement xSub1 = doc.CreateElement("result");
//                xSub1.InnerText = Convert.ToString(e);
//                xelen1.AppendChild(xSub1);
                return doc.InnerXml;
            }

            FileStream fs = new FileStream("C:\\downloadworklist.txt", FileMode.Append, FileAccess.Write);
            StreamWriter sr = new StreamWriter(fs);
            sr.WriteLine(datetime + " " + doc.InnerXml);//开始写入值
            sr.Close();
            fs.Close();

            return doc.InnerXml;
        }
		
		        /// <summary>  
        /// PC上传施工工单
        /// </summary>  
        /// <returns>返回成功信息</returns>
        public string PCuploadWork(string Work)
        {
            string aDeviceName = "";
            string aDeviceId = "";
            string aFrameNo = "";
            string aBoardNo = "";
            string aPortNo = "";
            string zDeviceName = "";
            string zDeviceId = "";
            string zFrameNo = "";
            string zBoardNo = "";
            string zPortNo = "";
            string OperateType = "";
            string SplittingRatio = "";
            string routeType = "";
            string workId = "";
            string datetime = "";

            datetime = DateTime.Now.ToString();
            FileStream fs = new FileStream("C:\\uploadwork.txt", FileMode.Append, FileAccess.Write);
            StreamWriter sr = new StreamWriter(fs);
            sr.WriteLine(datetime + Work);//开始写入值
            sr.Close();
            fs.Close();

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Work.Trim());
                XmlNodeList xnl = doc.GetElementsByTagName("opticalRoute");
                foreach (XmlNode node in xnl)
                {
                    XmlElement xe = (XmlElement)node;
                    aDeviceName = xe.SelectSingleNode("aDeviceName").InnerText.Trim();
                    aDeviceId = xe.SelectSingleNode("aDeviceId").InnerText.Trim();
                    aFrameNo = xe.SelectSingleNode("aFrameNo").InnerText.Trim();
                    aBoardNo = xe.SelectSingleNode("aBoardNo").InnerText.Trim();
                    aPortNo = xe.SelectSingleNode("aPortNo").InnerText.Trim();
                    zDeviceName = xe.SelectSingleNode("zDeviceName").InnerText.Trim();
                    zDeviceId = xe.SelectSingleNode("zDeviceId").InnerText.Trim();
                    zFrameNo = xe.SelectSingleNode("zFrameNo").InnerText.Trim();
                    zBoardNo = xe.SelectSingleNode("zBoardNo").InnerText.Trim();
                    zPortNo = xe.SelectSingleNode("zPortNo").InnerText.Trim();
                    OperateType = xe.SelectSingleNode("OperateType").InnerText.Trim();
                    SplittingRatio = xe.SelectSingleNode("SplittingRatio").InnerText.Trim();
                    routeType = xe.SelectSingleNode("routeType").InnerText.Trim();
                    workId = xe.SelectSingleNode("workId").InnerText.Trim();

                    SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                    sqlCon.ConnectionString = ConServerStr;
                    sqlCon.Open();
                    string sql = "insert into routeList  (aDeviceName,aDeviceId,aFrameNo,aBoardNo,aPortNo,zDeviceName,zDeviceId,zFrameNo,zBoardNo,zPortNo,OperateType,SplittingRatio,routeType,workId)"
                    + "values ('" + aDeviceName + "','" + aDeviceId + "','" + aFrameNo + "','" + aBoardNo + "','" + aPortNo + "','" + zDeviceName + "','" + zDeviceId + "','" + zFrameNo + "','" + zBoardNo + "','" + zPortNo + "','" + OperateType + "','" + SplittingRatio + "','" + routeType + "','" + workId + "')";
                    SqlCommand cmd = new SqlCommand(sql, sqlCon);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    sqlCon.Dispose();
                }
            }
            catch (Exception e)
            {
//                return (Convert.ToString(e));
                return "false";
            }
            return "true";
        }

		        /// <summary>
        /// 6.6下载施工工单
        /// </summary>
        /// <param name="Work"></param>
        /// <returns></returns>
        public string downloadWork(string Work)
        { 
            string workId = "";
            string datetime = "";

            datetime = DateTime.Now.ToString();
            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdecl = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xdoc.AppendChild(xdecl);
            XmlElement xelen = xdoc.CreateElement("response");
            xdoc.AppendChild(xelen);
            XmlElement xelen1 = xdoc.CreateElement("routeList");
            xelen.AppendChild(xelen1);

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Work.Trim());
                XmlNodeList xnl = doc.GetElementsByTagName("request");
                foreach (XmlNode node in xnl)
                {
                    workId = node["workId"].FirstChild.Value;
                }

                SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon.ConnectionString = ConServerStr;
                sqlCon.Open();
                string  sql = "select * from routeList where workId='" + workId + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                cmd.ExecuteNonQuery();
                SqlDataReader reader = cmd.ExecuteReader();
                while(reader.Read())
                {
                    XmlElement xel = xdoc.CreateElement("opticalRoute");
                    xelen1.AppendChild(xel);
                    XmlElement Sub1 = xdoc.CreateElement("aDeviceName");
                    Sub1.InnerText = reader["aDeviceName"].ToString();
                    xel.AppendChild(Sub1);
                    XmlElement Sub2 = xdoc.CreateElement("aDeviceId");
                    Sub2.InnerText = reader["aDeviceId"].ToString();
                    xel.AppendChild(Sub2);
                    XmlElement Sub3 = xdoc.CreateElement("aFrameNo");
                    Sub3.InnerText = reader["aFrameNo"].ToString();
                    xel.AppendChild(Sub3);
                    XmlElement Sub4 = xdoc.CreateElement("aBoardNo");
                    Sub4.InnerText = reader["aBoardNo"].ToString();
                    xel.AppendChild(Sub4);
                    XmlElement Sub5 = xdoc.CreateElement("aPortNo");
                    Sub5.InnerText = reader["aPortNo"].ToString();
                    xel.AppendChild(Sub5);
                    XmlElement Sub6 = xdoc.CreateElement("zDeviceName");
                    Sub6.InnerText = reader["zDeviceName"].ToString();
                    xel.AppendChild(Sub6);
                    XmlElement Sub7 = xdoc.CreateElement("zDeviceId");
                    Sub7.InnerText = reader["zDeviceId"].ToString();
                    xel.AppendChild(Sub7);
                    XmlElement Sub8 = xdoc.CreateElement("zFrameNo");
                    Sub8.InnerText = reader["zFrameNo"].ToString();
                    xel.AppendChild(Sub8);
                    XmlElement Sub9 = xdoc.CreateElement("zBoardNo");
                    Sub9.InnerText = reader["zBoardNo"].ToString();
                    xel.AppendChild(Sub9);
                    XmlElement Sub10 = xdoc.CreateElement("zPortNo");
                    Sub10.InnerText = reader["zPortNo"].ToString();
                    xel.AppendChild(Sub10);
                    XmlElement Sub11 = xdoc.CreateElement("OperateType");
                    Sub11.InnerText = reader["OperateType"].ToString();
                    xel.AppendChild(Sub11);
                    XmlElement Sub12 = xdoc.CreateElement("SplittingRatio");
                    Sub12.InnerText = reader["SplittingRatio"].ToString();
                    xel.AppendChild(Sub12);
                    XmlElement Sub13 = xdoc.CreateElement("routeType");
                    Sub13.InnerText = reader["routeType"].ToString();
                    xel.AppendChild(Sub13);
                }
                reader.Close();
                cmd.Dispose();
				sqlCon.Dispose();

                string sql1 = "delete from routeList where workId='" + workId + "'";
                SqlCommand cmd1 = new SqlCommand(sql1, sqlCon1);
                cmd1.ExecuteNonQuery();
                cmd1.Dispose();
            }
            catch (Exception e)
            {
                FileStream fs1 = new FileStream("C:\\iODHerror.txt", FileMode.Append, FileAccess.Write);
                StreamWriter sr1 = new StreamWriter(fs1);
                sr1.WriteLine(datetime + " downloadwork " + Convert.ToString(e));//开始写入值
                sr1.Close();
                fs1.Close();
//                XmlElement xSub1 = xdoc.CreateElement("result");
//                xSub1.InnerText = Convert.ToString(e);
//                xelen1.AppendChild(xSub1);
                return xdoc.InnerXml;
            }

            FileStream fs = new FileStream("C:\\downloadwork.txt", FileMode.Append, FileAccess.Write);
            StreamWriter sr = new StreamWriter(fs);
            sr.WriteLine(datetime + " " + xdoc.InnerXml);//开始写入值
            sr.Close();
            fs.Close();
            return xdoc.InnerXml;
        }
		
		
		        /// <summary>
        /// 6.4返回施工结果 
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public string uploadWorkResult(string xml)
        {
            string workorderId = "";
            string result = "";
            string aDeviceName = "";
            string aDeviceId  = "";
            string aFrameNo = "";
            string aBoardNo = "";
            string aPortNo = "";
            string AeletricalIdInfo = "";
            string AChangedStatus = "";
            string zDeviceName = "";
            string zDeviceId = "";
            string zFrameNo = "";
            string zBoardNo = "";
            string zPortNo = "";
            string ZeletricalIdInfo = "";
            string ZChangedStatus = "";
            string OperateType = "";
            string SplittingRatio = "";
            string RouteType = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml.Trim());
                XmlNodeList node1 = doc.GetElementsByTagName("request");
                foreach (XmlNode node in node1)
                {
                    workorderId = node["workorderId"].FirstChild.Value;
                    result = node["result"].FirstChild.Value;
                    SqlConnection sqlCon = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
                    sqlCon.ConnectionString = ConServerStr;
                    sqlCon.Open();
                    string sql = "insert into WorkResult1 (workorderId,result)" + "values ('" + workorderId + "','" + result +  "')";
                    SqlCommand cmd = new SqlCommand(sql, sqlCon);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    sqlCon.Dispose();//释放数据库连接资源
                }

                XmlNodeList node2 = doc.GetElementsByTagName("opticalRoute");
                if (node2.Count != 0)//xue修改：GetElementsByTagName("tagname")的返回值为NodeList，即使该节点不存在，返回值也不为空，不能用node2!=null来判断
                {
                    foreach (XmlNode node in node2)
                    {
                        XmlElement xe = (XmlElement)node;
                        aDeviceName = xe.SelectSingleNode("aDeviceName").InnerText.Trim();
                        aDeviceId = xe.SelectSingleNode("aDeviceId").InnerText.Trim();
                        aFrameNo = xe.SelectSingleNode("aFrameNo").InnerText.Trim();
                        aBoardNo = xe.SelectSingleNode("aBoardNo").InnerText.Trim();
                        aPortNo = xe.SelectSingleNode("aPortNo").InnerText.Trim();
                        //AeletricalIdInfo = xe.SelectSingleNode("AeletricalIdInfo").InnerText.Trim();
                        AeletricalIdInfo = xe.ChildNodes[5].InnerText.Trim();
                        //AChangedStatus = xe.SelectSingleNode("AChangedStatus").InnerText.Trim();
                        AChangedStatus = xe.ChildNodes[6].InnerText.Trim();
                        zDeviceName = xe.SelectSingleNode("zDeviceName").InnerText.Trim();
                        zDeviceId = xe.SelectSingleNode("zDeviceId").InnerText.Trim();
                        zFrameNo = xe.SelectSingleNode("zFrameNo").InnerText.Trim();
                        zBoardNo = xe.SelectSingleNode("zBoardNo").InnerText.Trim();
                        zPortNo = xe.SelectSingleNode("zPortNo").InnerText.Trim();
                      //  ZeletricalIdInfo = xe.SelectSingleNode("ZeletricalIdInfo").InnerText.Trim();
                        ZeletricalIdInfo = xe.ChildNodes[12].InnerText.Trim();
                        //ZChangedStatus = xe.SelectSingleNode("ZChangedStatus").InnerText.Trim();
                        ZChangedStatus = xe.ChildNodes[13].InnerText.Trim();
                        OperateType = xe.SelectSingleNode("OperateType").InnerText.Trim();
                        SplittingRatio = xe.SelectSingleNode("SplittingRatio").InnerText.Trim();
                        RouteType = xe.SelectSingleNode("routeType").InnerText.Trim();

                        SqlConnection sqlCon = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
                        sqlCon.ConnectionString = ConServerStr;
                        sqlCon.Open();
                        string sql = "insert into WorkResult2 (workorderId,aDeviceName,aDeviceId,aFrameNo,aBoardNo,aPortNo,AeletricalIdInfo,AChangedStatus,zDeviceName,zDeviceId,zFrameNo,zBoardNo,zPortNo,ZeletricalIdInfo,ZChangedStatus,OperateType,SplittingRatio,RouteType)"
                        + "values ('" + workorderId + "','" + aDeviceName + "','" + aDeviceId + "','" + aFrameNo + "','" + aBoardNo + "','" + aPortNo + "','" + AeletricalIdInfo + "','" + AChangedStatus + "','" + zDeviceName + "','" + zDeviceId + "','" + zFrameNo + "','" + zBoardNo + "','" + zPortNo + "','" + ZeletricalIdInfo + "','" + ZChangedStatus + "','" + OperateType + "','" + SplittingRatio + "','" + RouteType + "')";
                        SqlCommand cmd = new SqlCommand(sql, sqlCon);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        sqlCon.Dispose();//释放数据库连接资源
                    }
                }
                else
                {
                    string sql1 = "insert into WorkResult1 (workorderId,result)"
                            + "values ('" + workorderId + "','" + result + "')";
                    SqlCommand cmd2 = new SqlCommand(sql1, sqlCon1);
                    cmd2.ExecuteNonQuery();
                    cmd2.Dispose();
                }
            }
            catch(Exception e)
            {
                Boolean b = false;
                XmlDocument doc = new XmlDocument();
                XmlDeclaration xdecl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(xdecl);
                XmlElement xelen = doc.CreateElement("response");
                doc.AppendChild(xelen);
                XmlElement xSub1 = doc.CreateElement("result");
                xSub1.InnerText = Convert.ToString(e);
                xelen.AppendChild(xSub1);
                return doc.InnerXml;
            }
            Boolean a = true;
            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdec2 = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xdoc.AppendChild(xdec2);
            XmlElement xelen1 = xdoc.CreateElement("response");
            xdoc.AppendChild(xelen1);
            XmlElement xSub2 = xdoc.CreateElement("result");
            xSub2.InnerText = Convert.ToString(a);
            xelen1.AppendChild(xSub2);
            return xdoc.InnerXml;
        }
		
		
		        /// <summary>
        /// PC下载施工结果
        /// </summary>
        /// <returns></returns>
        public string selectAllWorkResult(string workID)
        {
            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdecl = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xdoc.AppendChild(xdecl);
            XmlElement xelen = xdoc.CreateElement("response");
            xdoc.AppendChild(xelen);
            try
            {
                SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon.ConnectionString = ConServerStr;
                sqlCon.Open();
                string sql = "select * from WorkResult1 where workorderId = '" + workID + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                cmd.ExecuteNonQuery();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    XmlElement Sub1 = xdoc.CreateElement("workorderId");
                    Sub1.InnerText = reader["workorderId"].ToString();
                    xelen.AppendChild(Sub1);
                    XmlElement Sub2 = xdoc.CreateElement("result");
                    Sub2.InnerText = reader["result"].ToString();;
                    xelen.AppendChild(Sub2);
                }
                reader.Close();                
                cmd.Dispose();
                sqlCon.Dispose();//释放数据库连接资源
                XmlElement Sub3 = xdoc.CreateElement("routeList");
                xelen.AppendChild(Sub3);
                SqlConnection sqlCon2 = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon2.ConnectionString = ConServerStr;
                sqlCon2.Open();
                string sql2 = "select * from WorkResult2 where workorderId = '" + workID + "'";
                SqlCommand cmd2 = new SqlCommand(sql2, sqlCon2);
                cmd2.ExecuteNonQuery();
                SqlDataReader reader2 = cmd2.ExecuteReader();
                while (reader2.Read())
                {
                    XmlElement Sub4 = xdoc.CreateElement("opticalRoute");
                    Sub3.AppendChild(Sub4);
                    XmlElement Sub5 = xdoc.CreateElement("aDeviceName");
                    Sub5.InnerText = reader2["aDeviceName"].ToString();
                    Sub4.AppendChild(Sub5);
                    XmlElement Sub6 = xdoc.CreateElement("aDeviceId");
                    Sub6.InnerText = reader2["aDeviceId"].ToString();
                    Sub4.AppendChild(Sub6);
                    XmlElement Sub7 = xdoc.CreateElement("aFrameNo");
                    Sub7.InnerText = reader2["aFrameNo"].ToString();
                    Sub4.AppendChild(Sub7);
                    XmlElement Sub8 = xdoc.CreateElement("aBoardNo");
                    Sub8.InnerText = reader2["aBoardNo"].ToString();
                    Sub4.AppendChild(Sub8);
                    XmlElement Sub9 = xdoc.CreateElement("aPortNo");
                    Sub9.InnerText = reader2["aPortNo"].ToString();
                    Sub4.AppendChild(Sub9);
                    XmlElement Sub10 = xdoc.CreateElement("eletricalIdInfo");
                    Sub10.InnerText = reader2["AeletricalIdInfo"].ToString();
                    Sub4.AppendChild(Sub10);
                    XmlElement Sub11 = xdoc.CreateElement("ChangedStatus");
                    Sub11.InnerText = reader2["AChangedStatus"].ToString();
                    Sub4.AppendChild(Sub11);
                    XmlElement Sub12 = xdoc.CreateElement("zDeviceName");
                    Sub12.InnerText = reader2["zDeviceName"].ToString();
                    Sub4.AppendChild(Sub12);
                    XmlElement Sub13 = xdoc.CreateElement("zDeviceId");
                    Sub13.InnerText = reader2["zDeviceId"].ToString();
                    Sub4.AppendChild(Sub13);
                    XmlElement Sub14 = xdoc.CreateElement("zFrameNo");
                    Sub14.InnerText = reader2["zFrameNo"].ToString();
                    Sub4.AppendChild(Sub14);
                    XmlElement Sub15 = xdoc.CreateElement("zBoardNo");
                    Sub15.InnerText = reader2["zBoardNo"].ToString();
                    Sub4.AppendChild(Sub15);
                    XmlElement Sub16 = xdoc.CreateElement("zPortNo");
                    Sub16.InnerText = reader2["zPortNo"].ToString();
                    Sub4.AppendChild(Sub16);
                    XmlElement Sub17 = xdoc.CreateElement("eletricalIdInfo");
                    Sub17.InnerText = reader2["ZeletricalIdInfo"].ToString();
                    Sub4.AppendChild(Sub17);
                    XmlElement Sub18 = xdoc.CreateElement("ChangedStatus");
                    Sub18.InnerText = reader2["ZChangedStatus"].ToString();
                    Sub4.AppendChild(Sub18);
                    XmlElement Sub19 = xdoc.CreateElement("OperateType");
                    Sub19.InnerText = reader2["OperateType"].ToString();
                    Sub4.AppendChild(Sub19);
                    XmlElement Sub20 = xdoc.CreateElement("SplittingRatio");
                    Sub20.InnerText = reader2["SplittingRatio"].ToString();
                    Sub4.AppendChild(Sub20);
                    XmlElement Sub21 = xdoc.CreateElement("routeType");
                    Sub21.InnerText = reader2["routeType"].ToString();
                    Sub4.AppendChild(Sub21);
                }
                reader2.Close();              
                cmd2.Dispose();
                sqlCon2.Dispose();//释放数据库连接资源

                //删除相应工单内容
                string sql5 = "delete from WorkResult1 where workorderId = '" + workID + "'";
                SqlCommand cmd5 = new SqlCommand(sql5, sqlCon1);
                cmd5.ExecuteNonQuery();
                cmd5.Dispose();
                string sql6 = "delete from WorkResult2 where workorderId = '" + workID + "'";
                SqlCommand cmd6 = new SqlCommand(sql6, sqlCon1);
                cmd6.ExecuteNonQuery();
                cmd6.Dispose();
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return xdoc.InnerXml;
        }
		
		
		
		        /// <summary>
        /// PC上传巡检工单
        /// </summary>
        /// <param name="InspectionWorkOrderList"></param>
        /// <returns></returns>
        public string PCuploadInspectionWorkOrderList(string InspectionWorkOrderList)
        {
            string workOrderId = "";
            string deviceName = "";
            string deviceID = "";
            string deviceType = "";
            string deviceSoftwareVersion = "";
            string deviceHardwareVersion = "";
            string inspectTime = "";
            string frameNo = "";
            string boardNo = "";
            string portNo = "";
            string electronicIdInfo = "";

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(InspectionWorkOrderList.Trim());
                XmlNodeList xnl = doc.GetElementsByTagName("workOrderList");
                foreach (XmlNode node in xnl)
                {
                    XmlElement xe = (XmlElement)node;
                    deviceName = xe.SelectSingleNode("deviceName").InnerText.Trim();
                    deviceID = xe.SelectSingleNode("deviceID").InnerText.Trim();
                    deviceType = xe.SelectSingleNode("deviceType").InnerText.Trim();
                    deviceSoftwareVersion = xe.SelectSingleNode("deviceSoftwareVersion").InnerText.Trim();
                    deviceHardwareVersion = xe.SelectSingleNode("deviceHardwareVersion").InnerText.Trim();
                    inspectTime = xe.SelectSingleNode("inspectTime").InnerText.Trim();
                    workOrderId = xe.SelectSingleNode("workOrderId").InnerText.Trim();


                    SqlConnection sqlCon = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
                    sqlCon.ConnectionString = ConServerStr;
                    sqlCon.Open();
                    string sql = "insert into workOrderList (deviceName,deviceID,deviceType,deviceSoftwareVersion,deviceHardwareVersion,inspectTime,workOrderId)"
                               + "values ('" + deviceName + "','" + deviceID + "','" + deviceType + "','" + deviceSoftwareVersion + "','" + deviceHardwareVersion + "','" + inspectTime + "','" + workOrderId + "')";
                    SqlCommand cmd = new SqlCommand(sql, sqlCon);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    sqlCon.Dispose();
                }

                XmlNodeList xnl2 = doc.GetElementsByTagName("resorceData");
                foreach (XmlNode node in xnl2)
                {
                    XmlElement xe = (XmlElement)node;
                    frameNo = xe.SelectSingleNode("frameNo").InnerText.Trim();
                    boardNo = xe.SelectSingleNode("boardNo").InnerText.Trim();
                    portNo = xe.SelectSingleNode("portNo").InnerText.Trim();
                    electronicIdInfo = xe.SelectSingleNode("electronicIdInfo").InnerText.Trim();

                    SqlConnection sqlCon = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
                    sqlCon.ConnectionString = ConServerStr;
                    sqlCon.Open();
                    string sql2 = "insert into resorceData (frameNo,boardNo,portNo,electronicIdInfo,workOrderId)"
                                + "values ('" + frameNo + "','" + boardNo + "','" + portNo + "','" + electronicIdInfo + "','" + workOrderId + "')";
                    SqlCommand cmd2 = new SqlCommand(sql2, sqlCon);
                    cmd2.ExecuteNonQuery();
                    cmd2.Dispose();
                    sqlCon.Dispose();
                }
            }
            catch (Exception e)
            {
                return "false";
//                return (Convert.ToString(e));
            }
            return "true";
        }
		
		
		
		        /// <summary>
        /// 6.9下载巡检工单
        /// </summary>
        /// <param name="WorkOrderList"></param>
        /// <returns></returns>
        public string DownLoadInspectionWorkOrderList(string WorkOrderList)
        {
            string workOrderId = "";

            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdecl = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xdoc.AppendChild(xdecl);
            XmlElement xelen = xdoc.CreateElement("response");
            xdoc.AppendChild(xelen);
            XmlElement xel = xdoc.CreateElement("workOrderList");
            xelen.AppendChild(xel);

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(WorkOrderList.Trim());
                XmlNodeList xnl = doc.GetElementsByTagName("request");
                foreach (XmlNode node in xnl)
                {
                    workOrderId = node["workOrderId"].FirstChild.Value;
                }
                SqlConnection sqlCon = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
                sqlCon.ConnectionString = ConServerStr;
                sqlCon.Open();
                string sql = "select * from workOrderList where workOrderId='" + workOrderId + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                cmd.ExecuteNonQuery();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {                    
                    XmlElement Sub1 = xdoc.CreateElement("deviceName");
                    Sub1.InnerText = reader["deviceName"].ToString();
                    xel.AppendChild(Sub1);
                    XmlElement Sub2 = xdoc.CreateElement("deviceID");
                    Sub2.InnerText = reader["deviceID"].ToString();
                    xel.AppendChild(Sub2);
                    XmlElement Sub3 = xdoc.CreateElement("deviceType");
                    Sub3.InnerText = reader["deviceType"].ToString();
                    xel.AppendChild(Sub3);
                    XmlElement Sub4 = xdoc.CreateElement("deviceSoftwareVersion");
                    Sub4.InnerText = reader["deviceSoftwareVersion"].ToString();
                    xel.AppendChild(Sub4);
                    XmlElement Sub5 = xdoc.CreateElement("deviceHardwareVersion");
                    Sub5.InnerText = reader["deviceHardwareVersion"].ToString();
                    xel.AppendChild(Sub5);
                    XmlElement Sub6 = xdoc.CreateElement("inspectTime");
                    Sub6.InnerText = reader["inspectTime"].ToString();
                    xel.AppendChild(Sub6);
                }
                reader.Close();  
                cmd.Dispose();
                sqlCon.Dispose();


                SqlConnection sqlCon2 = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
                sqlCon2.ConnectionString = ConServerStr;
                sqlCon2.Open();
                string sql2 = "select * from resorceData where workOrderId='" + workOrderId + "'";
                SqlCommand cmd2 = new SqlCommand(sql2, sqlCon2);
                cmd2.ExecuteNonQuery();
                SqlDataReader reader2 = cmd2.ExecuteReader();
                while(reader2.Read())
                {
                    XmlElement xel2 = xdoc.CreateElement("resorceData");
                    xel.AppendChild(xel2);
                    XmlElement Sub21 = xdoc.CreateElement("frameNo");
                    Sub21.InnerText = reader2["frameNo"].ToString();
                    xel2.AppendChild(Sub21);
                    XmlElement Sub22 = xdoc.CreateElement("boardNo");
                    Sub22.InnerText = reader2["boardNo"].ToString();
                    xel2.AppendChild(Sub22);
                    XmlElement Sub23 = xdoc.CreateElement("portNo");
                    Sub23.InnerText = reader2["portNo"].ToString();
                    xel2.AppendChild(Sub23);
                    XmlElement Sub24 = xdoc.CreateElement("electronicIdInfo");
                    Sub24.InnerText = reader2["electronicIdInfo"].ToString();
                    xel2.AppendChild(Sub24);
                }
                reader2.Close();
                cmd2.Dispose();
                sqlCon2.Dispose();
                
                //删除相应表单内容
                string sql5 = "delete from workOrderList where workOrderId='" + workOrderId + "'";
                SqlCommand cmd5 = new SqlCommand(sql5, sqlCon1);
                cmd5.ExecuteNonQuery();
                cmd5.Dispose();

                string sql6 = "delete from resorceData where workOrderId='" + workOrderId + "'";
                SqlCommand cmd6 = new SqlCommand(sql6, sqlCon1);
                cmd6.ExecuteNonQuery();
                cmd6.Dispose();
            }
            catch (Exception e)
            {
                XmlElement xSub31 = xdoc.CreateElement("result");
//                xSub31.InnerText = Convert.ToString(e);
                xSub31.InnerText = "false";
                xel.AppendChild(xSub31);
                return xdoc.InnerXml;
            }
            return xdoc.InnerXml;
        }
		
		
		        /// <summary>
        /// 6.10上传巡检结果
        /// </summary>
        /// <param name="InspectionResult"></param>
        /// <returns></returns>
        public string UploadInspectionResult(string InspectionResult)
        {
            string workOrderID = "";
            string executionResult = "";
            string deviceName = "";
            string deviceID = "";
            string deviceType = "";
            string deviceSoftwareVersion = "";
            string deviceHardwareVersion = "";
            string inspectTime = "";

            string frameNo = "";
            string boardNo = "";
            string portNo = "";
            string electronicIdInfo = "";

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(InspectionResult.Trim());
                XmlNodeList xnl1 = doc.GetElementsByTagName("request");
                foreach (XmlNode node in xnl1)
                {
                    XmlElement xe = (XmlElement)node;
                    workOrderID = xe.SelectSingleNode("workOrderID").InnerText.Trim();
                    executionResult = xe.SelectSingleNode("executionResult").InnerText.Trim();

                    SqlConnection sqlCon2 = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
                    sqlCon2.ConnectionString = ConServerStr;
                    sqlCon2.Open();
                    string sql = "insert into workOrderResult (workOrderID,executionResult)"
                                + "values ('" + workOrderID + "','" + executionResult + "')";
                    SqlCommand cmd = new SqlCommand(sql, sqlCon2);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    sqlCon2.Dispose();
                }
                XmlNodeList xnl2 = doc.GetElementsByTagName("workOrderList");
                foreach (XmlNode node in xnl2)
                {
                    XmlElement xe = (XmlElement)node;
                    deviceName = xe.SelectSingleNode("deviceName").InnerText.Trim();
                    deviceID = xe.SelectSingleNode("deviceID").InnerText.Trim();
                    deviceType = xe.SelectSingleNode("deviceType").InnerText.Trim();
                    deviceSoftwareVersion = xe.SelectSingleNode("deviceSoftwareVersion").InnerText.Trim();
                    deviceHardwareVersion = xe.SelectSingleNode("deviceHardwareVersion").InnerText.Trim();
                    inspectTime = xe.SelectSingleNode("inspectTime").InnerText.Trim();

                    XmlNodeList xnl3 = doc.GetElementsByTagName("resorceData");
                    foreach (XmlNode node1 in xnl3)
                    {
                        XmlElement xe1 = (XmlElement)node1;
                        frameNo = xe1.SelectSingleNode("frameNo").InnerText.Trim();
                        boardNo = xe1.SelectSingleNode("boardNo").InnerText.Trim();
                        portNo = xe1.SelectSingleNode("portNo").InnerText.Trim();
                        electronicIdInfo = xe1.SelectSingleNode("electronicIdInfo").InnerText.Trim();
                        SqlConnection sqlCon2 = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
                        sqlCon2.ConnectionString = ConServerStr;
                        sqlCon2.Open();
                        string sql2 = "insert into resorceData (frameNo,boardNo,portNo,electronicIdInfo,workOrderId)"
                                    + "values ('" + frameNo + "','" + boardNo + "','" + portNo + "','" + electronicIdInfo + "','" + workOrderID + "')";
                        SqlCommand cmd2 = new SqlCommand(sql2, sqlCon2);
                        cmd2.ExecuteNonQuery();
                        cmd2.Dispose();
                        sqlCon2.Dispose();
                    }

                    SqlConnection sqlCon3 = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
                    sqlCon3.ConnectionString = ConServerStr;
                    sqlCon3.Open();
                    string sql1 = "insert into workOrderList (workOrderId,deviceName,deviceID,deviceType,deviceSoftwareVersion,deviceHardwareVersion,inspectTime)"
                                + "values ('" + workOrderID + "','" + deviceName + "','" + deviceID + "','" + deviceType + "','" + deviceSoftwareVersion + "','" + deviceHardwareVersion + "','" + inspectTime + "')";
                    SqlCommand cmd1 = new SqlCommand(sql1, sqlCon3);
                    cmd1.ExecuteNonQuery();
                    cmd1.Dispose();
                    sqlCon3.Dispose();
                }

            }
            catch (Exception e)
            {
                XmlDocument xdoc2 = new XmlDocument();
                XmlDeclaration xdec2 = xdoc2.CreateXmlDeclaration("1.0", "UTF-8", null);
                xdoc2.AppendChild(xdec2);
                XmlElement xelen2 = xdoc2.CreateElement("response");
                xdoc2.AppendChild(xelen2);
                XmlElement Sub3 = xdoc2.CreateElement("result");
                Sub3.InnerText = "false";
                xelen2.AppendChild(Sub3);
                return xdoc2.InnerXml;
            }
            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdecl = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xdoc.AppendChild(xdecl);
            XmlElement xelen = xdoc.CreateElement("response");
            xdoc.AppendChild(xelen);
            XmlElement Sub4 = xdoc.CreateElement("result");
            Sub4.InnerText = "true";
            xelen.AppendChild(Sub4);
            return xdoc.InnerXml;
        }
		
       ///PC下载巡检结果
        /// </summary>
        /// <param name="workId"></param>
        /// <returns></returns>
        public string DownloadInspectionResult(string workId)
        {
            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdecl = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xdoc.AppendChild(xdecl);
            XmlElement xelen = xdoc.CreateElement("response");
            xdoc.AppendChild(xelen);
            try
            {
                SqlConnection sqlCon = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
                sqlCon.ConnectionString = ConServerStr;
                sqlCon.Open();
                string sql = "select * from workOrderResult where workOrderId='" + workId + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                cmd.ExecuteNonQuery();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    XmlElement Sub1 = xdoc.CreateElement("workOrderID");
                    Sub1.InnerText = reader["workOrderID"].ToString();
                    xelen.AppendChild(Sub1);
                    XmlElement Sub2 = xdoc.CreateElement("executionResult");
                    Sub2.InnerText = reader["executionResult"].ToString();
                    xelen.AppendChild(Sub2);
                }
                reader.Close();
                cmd.Dispose();
                sqlCon.Dispose();


                SqlConnection sqlCon2 = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
                sqlCon2.ConnectionString = ConServerStr;
                sqlCon2.Open();
                string sql2 = "select * from workOrderList where workOrderId='" + workId + "'";
                SqlCommand cmd2 = new SqlCommand(sql2, sqlCon2);
                cmd2.ExecuteNonQuery();
                SqlDataReader reader2 = cmd2.ExecuteReader();
                while (reader2.Read())
                {
                    XmlElement Sub3 = xdoc.CreateElement("workOrderList");
                    xelen.AppendChild(Sub3);
                    XmlElement Sub31 = xdoc.CreateElement("deviceName");
                    Sub31.InnerText = reader2["deviceName"].ToString();
                    Sub3.AppendChild(Sub31);
                    XmlElement Sub32 = xdoc.CreateElement("deviceID");
                    Sub32.InnerText = reader2["deviceID"].ToString();
                    Sub3.AppendChild(Sub32);
                    XmlElement Sub33 = xdoc.CreateElement("deviceType");
                    Sub33.InnerText = reader2["deviceType"].ToString();
                    Sub3.AppendChild(Sub33);
                    XmlElement Sub34 = xdoc.CreateElement("deviceSoftwareVersion");
                    Sub34.InnerText = reader2["deviceSoftwareVersion"].ToString();
                    Sub3.AppendChild(Sub34);
                    XmlElement Sub35 = xdoc.CreateElement("deviceHardwareVersion");
                    Sub35.InnerText = reader2["deviceHardwareVersion"].ToString();
                    Sub3.AppendChild(Sub35);
                    XmlElement Sub36 = xdoc.CreateElement("inspectTime");
                    Sub36.InnerText = reader2["inspectTime"].ToString();
                    Sub3.AppendChild(Sub36);
                    

                    SqlConnection sqlCon3 = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
                    sqlCon3.ConnectionString = ConServerStr;
                    sqlCon3.Open();
                    string sql3 = "select * from resorceData where workOrderId='" + workId + "'";
                    SqlCommand cmd3 = new SqlCommand(sql3, sqlCon3);
                    cmd3.ExecuteNonQuery();
                    SqlDataReader reader3 = cmd3.ExecuteReader();
                    while (reader3.Read())
                    {
                        XmlElement Sub37 = xdoc.CreateElement("resorceData");
                        Sub3.AppendChild(Sub37);
                        XmlElement Sub371 = xdoc.CreateElement("frameNo");
                        Sub371.InnerText = reader3["frameNo"].ToString();
                        Sub37.AppendChild(Sub371);
                        XmlElement Sub372 = xdoc.CreateElement("boardNo");
                        Sub372.InnerText = reader3["boardNo"].ToString();
                        Sub37.AppendChild(Sub372);
                        XmlElement Sub373 = xdoc.CreateElement("portNo");
                        Sub373.InnerText = reader3["portNo"].ToString();
                        Sub37.AppendChild(Sub373);
                        XmlElement Sub374 = xdoc.CreateElement("electronicIdInfo");
                        Sub374.InnerText = reader3["electronicIdInfo"].ToString();
                        Sub37.AppendChild(Sub374);
                    }
                    reader3.Close();
                    cmd3.Dispose();
                    sqlCon3.Dispose();
                }
                reader2.Close();
                cmd2.Dispose();
                sqlCon2.Dispose();

                string sql4 = "delete from workOrderResult where workOrderID='" + workId + "'";
                SqlCommand cmd4 = new SqlCommand(sql4, sqlCon1);
                cmd4.ExecuteNonQuery();
                cmd4.Dispose();

                string sql5 = "delete from workOrderList where workOrderId='" + workId + "'";
                SqlCommand cmd5 = new SqlCommand(sql5, sqlCon1);
                cmd5.ExecuteNonQuery();
                cmd5.Dispose();

                string sql6 = "delete from resorceData where workOrderId='" + workId + "'";
                SqlCommand cmd6 = new SqlCommand(sql6, sqlCon1);
                cmd6.ExecuteNonQuery();
                cmd6.Dispose();
            }
            catch (Exception e)
            {
                XmlElement xsub = xdoc.CreateElement("result");
                xsub.InnerText = "false";
                xelen.AppendChild(xsub);
                return xdoc.InnerXml;
            }
            return xdoc.InnerXml;
        }
		
		
		    /// <summary>
        /// 6.7上传设备信息               
        /// </summary>
        /// <param name="Device"></param>
        /// <returns></returns>
        public string uploadDevice(string Device)
        {
            string id = "";
            string type = "";
            string vendorId = "";
            string vendorDeviceType = "";

            string deviceId = "";
            string shelfNo = "";

            string moduleNo = "";

            string termNo = "";
            string patchElecID = "";
            string linkElecID = "";

            XmlDocument adoc = new XmlDocument();
            adoc.LoadXml(Device.Trim());
            try
            {
                XmlNodeList xnl1 = adoc.GetElementsByTagName("device");
                    foreach (XmlNode node1 in xnl1)
                    {
                        id = node1.Attributes["id"].Value;
                        type = node1.Attributes["type"].Value;
                        vendorId = node1.Attributes["vendorId"].Value;
                        vendorDeviceType = node1.Attributes["vendorDeviceType"].Value;
						
						SqlConnection sqlCon = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
						sqlCon.ConnectionString = ConServerStr;
						sqlCon.Open();
                        string sql = "insert into device_message (id,type,vendorId,vendorDeviceType)"
                            + "values ('" + id + "','" + type + "','" + vendorId + "','" + vendorDeviceType + "')";
                        SqlCommand cmd = new SqlCommand(sql, sqlCon);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
						sqlCon.Dispose();
                    }
                XmlNodeList xnl2 = adoc.GetElementsByTagName("Shelf");
                    foreach (XmlNode node2 in xnl2)
                    {
                        deviceId = node2.Attributes["deviceId"].Value;
                        shelfNo = node2.Attributes["shelfNo"].Value;
						
						SqlConnection sqlCon = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
						sqlCon.ConnectionString = ConServerStr;
						sqlCon.Open();
                        string sql = "insert into shelf_message(deviceId,shelfNo)"
                            + "values ('" + deviceId + "','" + shelfNo + "')";
                        SqlCommand cmd = new SqlCommand(sql, sqlCon);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
						sqlCon.Dispose();
                    }
               
                XmlNodeList xnl3 = adoc.GetElementsByTagName("Module");
                    foreach (XmlNode node3 in xnl3)
                    {
                        deviceId = node3.Attributes["deviceId"].Value;
                        shelfNo = node3.Attributes["shelfNo"].Value;
                        moduleNo = node3.Attributes["moduleNo"].Value;
						
						SqlConnection sqlCon = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
						sqlCon.ConnectionString = ConServerStr;
						sqlCon.Open();
                        string sql = "insert into Module_message(deviceId,shelfNo,moduleNo)"
                            + "values ('" + deviceId + "','" + shelfNo + "','" + moduleNo + "')";
                        SqlCommand cmd = new SqlCommand(sql, sqlCon);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
						sqlCon.Dispose();
                    }
                XmlNodeList xnl4 = adoc.GetElementsByTagName("Term");
                    foreach (XmlNode node4 in xnl4)
                    {
                        deviceId = node4.Attributes["deviceId"].Value;
                        shelfNo = node4.Attributes["shelfNo"].Value;
                        moduleNo = node4.Attributes["moduleNo"].Value;
                        termNo = node4.Attributes["termNo"].Value;
                        patchElecID = node4.Attributes["patchElecID"].Value;
                        linkElecID = node4.Attributes["linkElecID"].Value;
						
						SqlConnection sqlCon = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
						sqlCon.ConnectionString = ConServerStr;
						sqlCon.Open();
                        string sql = "insert into Term_message(deviceId,shelfNo,moduleNo,termNo,patchElecID,linkElecID)"
                            + "values ('" + deviceId + "','" + shelfNo + "','" + moduleNo + "','" + termNo + "','" + patchElecID + "','" + linkElecID + "')";
                        SqlCommand cmd = new SqlCommand(sql, sqlCon);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
						sqlCon.Dispose();
                    }
                
            }
            catch (Exception e)
            {
                XmlDocument doc = new XmlDocument();
                XmlDeclaration xdecl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(xdecl);
                XmlElement xelen = doc.CreateElement("response");
                doc.AppendChild(xelen);
                XmlElement xSub1 = doc.CreateElement("result");
                xSub1.InnerText = Convert.ToString(e);
                xelen.AppendChild(xSub1);
                return doc.InnerXml;
            }
            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdec2 = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xdoc.AppendChild(xdec2);
            XmlElement xelen1 = xdoc.CreateElement("response");
            xdoc.AppendChild(xelen1);
            XmlElement xel = xdoc.CreateElement("result");
            xelen1.AppendChild(xel);
            XmlElement xSub = xdoc.CreateElement("isSuccess");
            xSub.InnerText = "true";
            xel.AppendChild(xSub);
            XmlElement xSub2 = xdoc.CreateElement("failReason");
            xSub2.InnerText = "null";
            xel.AppendChild(xSub2);
            XmlElement xSub3 = xdoc.CreateElement("remark");
            xSub3.InnerText = "null";
            xel.AppendChild(xSub3);
            return xdoc.InnerXml;
        }
		
		
		        /// <summary>
        /// PC下载设备信息
        /// </summary>
        /// <returns></returns>
        public string PCDownLoadAllDevice()
        {
            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdecl = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xdoc.AppendChild(xdecl);
            XmlElement xelen = xdoc.CreateElement("response");
            xdoc.AppendChild(xelen);
            XmlElement xelen1 = xdoc.CreateElement("inODNInventory");
            xelen.AppendChild(xelen1);
            try
            {
                XmlElement xelen2 = xdoc.CreateElement("devices");
                xelen1.AppendChild(xelen2);
				
				SqlConnection sqlCon = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
				sqlCon.ConnectionString = ConServerStr;
				sqlCon.Open();
                string sql = "select * from device_message";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                cmd.ExecuteNonQuery();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    XmlElement Sub = xdoc.CreateElement("device");
                    xelen2.AppendChild(Sub);
                    XmlAttribute suttitle1 = xdoc.CreateAttribute("id");
                    suttitle1.Value = reader["id"].ToString();
                    Sub.Attributes.Append(suttitle1);
                    XmlAttribute suttitle2 = xdoc.CreateAttribute("type");
                    suttitle2.Value = reader["type"].ToString();
                    Sub.Attributes.Append(suttitle2);
                    XmlAttribute suttitle3 = xdoc.CreateAttribute("vendorId");
                    suttitle3.Value = reader["vendorId"].ToString();
                    Sub.Attributes.Append(suttitle3);
                    XmlAttribute suttitle4 = xdoc.CreateAttribute("vendorDeviceType");
                    suttitle4.Value = reader["vendorDeviceType"].ToString();
                    Sub.Attributes.Append(suttitle4);
                }
                reader.Close();
                cmd.Dispose();
				sqlCon.Dispose();

                XmlElement xelen3 = xdoc.CreateElement("Shelfs");
                xelen1.AppendChild(xelen3);
				
				SqlConnection sqlCon2 = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
				sqlCon2.ConnectionString = ConServerStr;
				sqlCon2.Open();
                string sql1 = "select * from shelf_message";
                SqlCommand cmd1 = new SqlCommand(sql1, sqlCon2);
                cmd1.ExecuteNonQuery();
                SqlDataReader reader1 = cmd1.ExecuteReader();
                while (reader1.Read())
                {
                    XmlElement Sub = xdoc.CreateElement("Shelf");
                    xelen3.AppendChild(Sub);
                    XmlAttribute suttitle1 = xdoc.CreateAttribute("deviceId");
                    suttitle1.Value = reader1["deviceId"].ToString();
                    Sub.Attributes.Append(suttitle1);
                    XmlAttribute suttitle2 = xdoc.CreateAttribute("shelfNo");
                    suttitle2.Value = reader1["shelfNo"].ToString();
                    Sub.Attributes.Append(suttitle2);
                }
                reader1.Close();
                cmd1.Dispose();
				sqlCon2.Dispose();

                XmlElement xelen4 = xdoc.CreateElement("Modules");
                xelen1.AppendChild(xelen4);
				
				SqlConnection sqlCon3 = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
				sqlCon3.ConnectionString = ConServerStr;
				sqlCon3.Open();
                string sql2 = "select * from Module_message";
                SqlCommand cmd2 = new SqlCommand(sql2, sqlCon3);
                cmd2.ExecuteNonQuery();
                SqlDataReader reader2 = cmd2.ExecuteReader();
                while (reader2.Read())
                {
                    XmlElement Sub = xdoc.CreateElement("Shelf");
                    xelen4.AppendChild(Sub);
                    XmlAttribute suttitle1 = xdoc.CreateAttribute("deviceId");
                    suttitle1.Value = reader2["deviceId"].ToString();
                    Sub.Attributes.Append(suttitle1);
                    XmlAttribute suttitle2 = xdoc.CreateAttribute("shelfNo");
                    suttitle2.Value = reader2["shelfNo"].ToString();
                    Sub.Attributes.Append(suttitle2);
                    XmlAttribute suttitle3 = xdoc.CreateAttribute("moduleNo");
                    suttitle3.Value = reader2["moduleNo"].ToString();
                    Sub.Attributes.Append(suttitle3);
                }
                reader2.Close();
                cmd2.Dispose();
				sqlCon3.Dispose();

                XmlElement xelen5 = xdoc.CreateElement("Terms");
                xelen1.AppendChild(xelen5);
				
				SqlConnection sqlCon4 = new SqlConnection();//webservice本身能支持并行访问，每个接口都需访问数据库，若都是用全局连接则客户端并行访问时会出现冲突，所以每次都新建一个数据库连接，使用完就释放资源
				sqlCon4.ConnectionString = ConServerStr;
				sqlCon4.Open();
                string sql3 = "select * from Term_message";
                SqlCommand cmd3 = new SqlCommand(sql3, sqlCon4);
                cmd3.ExecuteNonQuery();
                SqlDataReader reader3 = cmd3.ExecuteReader();
                while (reader3.Read())
                {
                    XmlElement Sub = xdoc.CreateElement("Term");
                    xelen5.AppendChild(Sub);
                    XmlAttribute suttitle1 = xdoc.CreateAttribute("deviceId");
                    suttitle1.Value = reader3["deviceId"].ToString();
                    Sub.Attributes.Append(suttitle1);
                    XmlAttribute suttitle2 = xdoc.CreateAttribute("shelfNo");
                    suttitle2.Value = reader3["shelfNo"].ToString();
                    Sub.Attributes.Append(suttitle2);
                    XmlAttribute suttitle3 = xdoc.CreateAttribute("moduleNo");
                    suttitle3.Value = reader3["moduleNo"].ToString();
                    Sub.Attributes.Append(suttitle3);
                    XmlAttribute suttitle4 = xdoc.CreateAttribute("termNo");
                    suttitle4.Value = reader3["termNo"].ToString();
                    Sub.Attributes.Append(suttitle4);
                    XmlAttribute suttitle5 = xdoc.CreateAttribute("patchElecID");
                    suttitle5.Value = reader3["patchElecID"].ToString();
                    Sub.Attributes.Append(suttitle5);
                    XmlAttribute suttitle6 = xdoc.CreateAttribute("linkElecID");
                    suttitle6.Value = reader3["linkElecID"].ToString();
                    Sub.Attributes.Append(suttitle6);
                }
                reader3.Close();
                cmd3.Dispose();
				sqlCon4.Dispose();

                deletemessage();//xue添加：删除数据库数据
            }
            catch (Exception e)
            {
                XmlElement xSub31 = xdoc.CreateElement("result");
                xSub31.InnerText = Convert.ToString(e);
                xelen.AppendChild(xSub31);
                return xdoc.InnerXml;
            }
            return xdoc.InnerXml;
        }    
		
		
		
		        /// <summary>  
        /// 6.2上传告警信息  
        /// </summary>  
        /// <returns>响应结果</returns>
        public string uploadDeviceAlarms(string DAmessage)
        {
            string OID = "";
            string deviceId = ""; ;
            string boxNo = "";
            string diskNo = "";
            string portNo = "";
            string alarmType = "";
            XmlDocument adoc = new XmlDocument();
            adoc.LoadXml(DAmessage.Trim());
            try
            {
                XmlNodeList xnl = adoc.GetElementsByTagName("alarm");
                foreach (XmlNode node in xnl)
                {
                    OID = node["OID"].FirstChild.Value;
                    deviceId = node["deviceId"].FirstChild.Value;
                    boxNo = node["boxNo"].FirstChild.Value;
                    diskNo = node["diskNo"].FirstChild.Value;
                    portNo = node["portNo"].FirstChild.Value;
                    alarmType = node["alarmType"].FirstChild.Value;

                    string sql = "insert into alarmList (OID,deviceId,boxNo,diskNo,portNo,alarmType) values ('" + OID + "','" + deviceId + "','" + boxNo + "','" + diskNo + "','" + portNo + "','" + alarmType + "')";
                    SqlCommand cmd = new SqlCommand(sql, sqlCon1);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                return "false";
            }

            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdecl = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xdoc.AppendChild(xdecl);
            XmlElement xelen = xdoc.CreateElement("response");
            xdoc.AppendChild(xelen);
            XmlElement xel = xdoc.CreateElement("result");
            xelen.AppendChild(xel);
            XmlElement xSub1 = xdoc.CreateElement("isSuccess");
            xSub1.InnerText = "sohstx";
            xel.AppendChild(xSub1);
            XmlElement xSub2 = xdoc.CreateElement("failReason");
            xSub2.InnerText = "12";
            xel.AppendChild(xSub2);
            XmlElement xSub3 = xdoc.CreateElement("remark");
            xSub3.InnerText = "acca";
            xel.AppendChild(xSub3);

            return xdoc.InnerXml;
        }
		
		        /// <summary>
        /// PC下载告警信息
        /// </summary>
        /// <param name="DeviceAlarms"></param>
        /// <returns></returns>
        public string PCDownLoadDeviceAlarms()
        {
            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdecl = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xdoc.AppendChild(xdecl);
            XmlElement xelen = xdoc.CreateElement("response");
            xdoc.AppendChild(xelen);
            XmlElement xelen1 = xdoc.CreateElement("alarmList");
            xelen.AppendChild(xelen1);

            try
            {
                SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon.ConnectionString = ConServerStr;
                sqlCon.Open();
                string sql = "select * from alarmList";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                cmd.ExecuteNonQuery();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    XmlElement Sub = xdoc.CreateElement("alarm");
                    xelen1.AppendChild(Sub);
                    XmlElement Sub1 = xdoc.CreateElement("OID");
                    Sub1.InnerText = reader["OID"].ToString();
                    Sub.AppendChild(Sub1);
                    XmlElement Sub2 = xdoc.CreateElement("deviceId");
                    Sub2.InnerText = reader["deviceId"].ToString();
                    Sub.AppendChild(Sub2);
                    XmlElement Sub3 = xdoc.CreateElement("boxNo");
                    Sub3.InnerText = reader["boxNo"].ToString();
                    Sub.AppendChild(Sub3);
                    XmlElement Sub4 = xdoc.CreateElement("diskNo");
                    Sub4.InnerText = reader["diskNo"].ToString();
                    Sub.AppendChild(Sub4);
                    XmlElement Sub5 = xdoc.CreateElement("portNo");
                    Sub5.InnerText = reader["portNo"].ToString();
                    Sub.AppendChild(Sub5);
                    XmlElement Sub6 = xdoc.CreateElement("alarmType");
                    Sub6.InnerText = reader["alarmType"].ToString();
                    Sub.AppendChild(Sub6);
                }
                reader.Close();
                cmd.Dispose();
                sqlCon.Dispose();

                //删除相应工单内容
                string sql5 = "delete from alarmList";
                SqlCommand cmd5 = new SqlCommand(sql5, sqlCon1);
                cmd5.ExecuteNonQuery();
                cmd5.Dispose();
            }
            catch (Exception e)
            {
                return "false";
            }
            return xdoc.InnerXml;
        }
		
		
		        /// <summary>
        /// 6.11数据资源收集工单
        /// </summary>
        /// <param name="WorkOrder"></param>
        /// <returns></returns>
        public string ResourceDataCollectionWorkOrder(string WorkOrder)
        {
            string workOrderId = "";

            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdecl = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xdoc.AppendChild(xdecl);
            XmlElement xelen = xdoc.CreateElement("response");
            xdoc.AppendChild(xelen);
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(WorkOrder.Trim());
                XmlNodeList xnl = doc.GetElementsByTagName("request");
                foreach (XmlNode node in xnl)
                {
                    XmlElement xe = (XmlElement)node;
                    workOrderId = xe.SelectSingleNode("workOrderId").InnerText.Trim();
                }

                SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon.ConnectionString = ConServerStr;
                sqlCon.Open();
                string sql = "select * from ResourceDataCollectionWorkOrder where workOrderId='" + workOrderId + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                cmd.ExecuteNonQuery();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    XmlElement xel = xdoc.CreateElement("workOrderList");
                    xelen.AppendChild(xel);
                    XmlElement Sub1 = xdoc.CreateElement("vendorID");
                    Sub1.InnerText = reader["vendorID"].ToString();
                    xel.AppendChild(Sub1);
                    XmlElement Sub2 = xdoc.CreateElement("deviceID");
                    Sub2.InnerText = reader["deviceID"].ToString();
                    xel.AppendChild(Sub2);
                }
                reader.Close();
                cmd.Dispose();
                sqlCon.Dispose();
                /*
                //删除相应工单内容
                string sql5 = "delete from ResourceDataCollectionWorkOrder where workOrderId='" + workOrderId + "'";
                SqlCommand cmd5 = new SqlCommand(sql5, sqlCon1);
                cmd5.ExecuteNonQuery();
                cmd5.Dispose();*/
            }
            catch (Exception e)
            {
                XmlElement xSub31 = xdoc.CreateElement("result");
                xSub31.InnerText = Convert.ToString(e);
                xelen.AppendChild(xSub31);
                return xdoc.InnerXml;
            }
            return xdoc.InnerXml;
        }
		
		
		        /// <summary>
        /// PC上传数据资源采集工单
        /// </summary>
        /// <param name="ResourceDataCollectionWorkOrder"></param>
        /// <returns></returns>
        public string PCuploadResourceDataCollectionWorkOrder(string ResourceDataCollectionWorkOrder)
        {
            string vendorID = "";
            string deviceID = "";
            string workOrderId = "";

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(ResourceDataCollectionWorkOrder.Trim());
                XmlNodeList xnl = doc.GetElementsByTagName("workOrderList");
                foreach (XmlNode node in xnl)
                {
                    XmlElement xe = (XmlElement)node;
                    vendorID = xe.SelectSingleNode("vendorID").InnerText.Trim();
                    deviceID = xe.SelectSingleNode("deviceID").InnerText.Trim();
                    workOrderId = xe.SelectSingleNode("workOrderId").InnerText.Trim();

                    SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                    sqlCon.ConnectionString = ConServerStr;
                    sqlCon.Open();
                    string sql = "insert into ResourceDataCollectionWorkOrder (vendorID,deviceID,workOrderId)"
                               + "values ('" + vendorID + "','" + deviceID + "','" + workOrderId + "')";
                    SqlCommand cmd = new SqlCommand(sql, sqlCon);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    sqlCon.Dispose();
                }
            }
            catch (Exception e)
            {
                return (Convert.ToString(e));
            }
            return "true";
        }
		
		
		        /// <summary>
        /// 6.12下载配置工单
        /// </summary>
        /// <param name="ConfigurationWorkOrder"></param>
        /// <returns></returns>
        public string DownloadConfigurationWorkOrder(string ConfigurationWorkOrder)
        {
            string workOrderId = "";

            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdecl = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xdoc.AppendChild(xdecl);
            XmlElement xelen = xdoc.CreateElement("response");
            xdoc.AppendChild(xelen);
            XmlElement xel = xdoc.CreateElement("workOrderList");
            xelen.AppendChild(xel);
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(ConfigurationWorkOrder.Trim());
                XmlNodeList xnl = doc.GetElementsByTagName("request");
                foreach (XmlNode node in xnl)
                {
                    workOrderId = node["workOrderId"].FirstChild.Value;
                }

                SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon.ConnectionString = ConServerStr;
                sqlCon.Open();
                string sql = "select * from ConfigurationWorkOrder where workOrderId='" + workOrderId + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                cmd.ExecuteNonQuery();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    XmlElement Sub1 = xdoc.CreateElement("deviceName");
                    Sub1.InnerText = reader["deviceName"].ToString();
                    xel.AppendChild(Sub1);
                    XmlElement Sub2 = xdoc.CreateElement("deviceID");
                    Sub2.InnerText = reader["deviceID"].ToString();
                    xel.AppendChild(Sub2);
                    XmlElement Sub3 = xdoc.CreateElement("deviceType");
                    Sub3.InnerText = reader["deviceType"].ToString();
                    xel.AppendChild(Sub3);
                    XmlElement Sub4 = xdoc.CreateElement("deviceIPAddr");
                    Sub4.InnerText = reader["deviceIPAddr"].ToString();
                    xel.AppendChild(Sub4);
                    XmlElement Sub5 = xdoc.CreateElement("deviceIPAddrMask");
                    Sub5.InnerText = reader["deviceIPAddrMask"].ToString();
                    xel.AppendChild(Sub5);
                    XmlElement Sub6 = xdoc.CreateElement("deviceIPGateway");
                    Sub6.InnerText = reader["deviceIPGateway"].ToString();
                    xel.AppendChild(Sub6);
                    XmlElement Sub7 = xdoc.CreateElement("NMSIPAddr");
                    Sub7.InnerText = reader["NMSIPAddr"].ToString();
                    xel.AppendChild(Sub7);
                    XmlElement Sub8 = xdoc.CreateElement("NMSTrapPort");
                    Sub8.InnerText = reader["NMSTrapPort"].ToString();
                    xel.AppendChild(Sub8);
                    XmlElement Sub9 = xdoc.CreateElement("NMSTrapEnable");
                    Sub9.InnerText = reader["NMSTrapEnable"].ToString();
                    xel.AppendChild(Sub9);
                    XmlElement Sub10 = xdoc.CreateElement("NMSTrapSecurityName");
                    Sub10.InnerText = reader["NMSTrapSecurityName"].ToString();
                    xel.AppendChild(Sub10);
                    XmlElement Sub11 = xdoc.CreateElement("SNMPGroupName");
                    Sub11.InnerText = reader["SNMPGroupName"].ToString();
                    xel.AppendChild(Sub11);
                    XmlElement Sub12 = xdoc.CreateElement("SNMPAuthority");
                    Sub12.InnerText = reader["SNMPAuthority"].ToString();
                    xel.AppendChild(Sub12);
                    XmlElement Sub13 = xdoc.CreateElement("SNMPViewEnable");
                    Sub13.InnerText = reader["SNMPViewEnable"].ToString();
                    xel.AppendChild(Sub13);
                    XmlElement Sub14 = xdoc.CreateElement("SNMPViewName");
                    Sub14.InnerText = reader["SNMPViewName"].ToString();
                    xel.AppendChild(Sub14);
                }
                reader.Close();               
                cmd.Dispose();
                sqlCon.Dispose();

                //删除相应工单内容
                string sql5 = "delete from ConfigurationWorkOrder where workOrderId='" + workOrderId + "'";
                SqlCommand cmd5 = new SqlCommand(sql5, sqlCon1);
                cmd5.ExecuteNonQuery();
                cmd5.Dispose();

            }
            catch (Exception e)
            {
                XmlElement xSub31 = xdoc.CreateElement("result");
//                xSub31.InnerText = Convert.ToString(e);
                xSub31.InnerText="false";
                xelen.AppendChild(xSub31);
                return xdoc.InnerXml;
            }
            return xdoc.InnerXml;
        }
		
		
		        /// <summary>  
        /// PC上传配置工单
        /// </summary>  
        /// <returns>返回成功信息</returns>
        public string PCuploadConfigurationWorkOrder(string ConfigurationWorkOrder)
        {
            string deviceName = "";
            string deviceID = "";
            string deviceType = "";
            string deviceIPAddr = "";
            string deviceIPAddrMask = "";
            string deviceIPGateway = "";
            string NMSIPAddr = "";
            string NMSTrapPort = "";
            string NMSTrapEnable = "";
            string NMSTrapSecurityName = "";
            string SNMPGroupName = "";
            string SNMPAuthority = "";
            string SNMPViewEnable = "";
            string SNMPViewName = "";
            string WorkOrderId = "";

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(ConfigurationWorkOrder.Trim());
                XmlNodeList xnl = doc.GetElementsByTagName("workOrderList");
                foreach (XmlNode node in xnl)
                {
                    XmlElement xe = (XmlElement)node;
                    deviceName = xe.SelectSingleNode("deviceName").InnerText.Trim();
                    deviceID = xe.SelectSingleNode("deviceID").InnerText.Trim();
                    deviceType = xe.SelectSingleNode("deviceType").InnerText.Trim();
                    deviceIPAddr = xe.SelectSingleNode("deviceIPAddr").InnerText.Trim();
                    deviceIPAddrMask = xe.SelectSingleNode("deviceIPAddrMask").InnerText.Trim();
                    deviceIPGateway = xe.SelectSingleNode("deviceIPGateway").InnerText.Trim();
                    NMSIPAddr = xe.SelectSingleNode("NMSIPAddr").InnerText.Trim();
                    NMSTrapPort = xe.SelectSingleNode("NMSTrapPort").InnerText.Trim();
                    NMSTrapEnable = xe.SelectSingleNode("NMSTrapEnable").InnerText.Trim();
                    NMSTrapSecurityName = xe.SelectSingleNode("NMSTrapSecurityName").InnerText.Trim();
                    SNMPGroupName = xe.SelectSingleNode("SNMPGroupName").InnerText.Trim();
                    SNMPAuthority = xe.SelectSingleNode("SNMPAuthority").InnerText.Trim();
                    SNMPViewEnable = xe.SelectSingleNode("SNMPViewEnable").InnerText.Trim();
                    SNMPViewName = xe.SelectSingleNode("SNMPViewName").InnerText.Trim();
                    WorkOrderId = xe.SelectSingleNode("WorkOrderId").InnerText.Trim();

                    SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                    sqlCon.ConnectionString = ConServerStr;
                    sqlCon.Open();
                    string sql = "insert into ConfigurationWorkOrder  (deviceName,deviceID,deviceType,deviceIPAddr,deviceIPAddrMask,deviceIPGateway,NMSIPAddr,NMSTrapPort,NMSTrapEnable,NMSTrapSecurityName,SNMPGroupName,SNMPAuthority,SNMPViewEnable,SNMPViewName,WorkOrderId)"
                    + "values ('" + deviceName + "','" + deviceID + "','" + deviceType + "','" + deviceIPAddr + "','" + deviceIPAddrMask + "','" + deviceIPGateway + "','" + NMSIPAddr + "','" + NMSTrapPort + "','" + NMSTrapEnable + "','" + NMSTrapSecurityName + "','" + SNMPGroupName + "','" + SNMPAuthority + "','" + SNMPViewEnable + "','" + SNMPViewName + "','" + WorkOrderId + "')";
                    SqlCommand cmd = new SqlCommand(sql, sqlCon);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    sqlCon.Dispose();
                }
            }
            catch (Exception e)
            {

                return "false";

            }
            return "true";

        }


        /// <summary>
        /// 6.13下载电子标签写入工单
        /// </summary>
        /// <param name="WorkOrder"></param>
        /// <returns></returns>
        public string ElectricalIDWritingWorkOrder(string WorkOrder)
        {
            string workOrderId = "";

            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdecl = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xdoc.AppendChild(xdecl);
            XmlElement xelen = xdoc.CreateElement("response");
            xdoc.AppendChild(xelen);
            XmlElement xel = xdoc.CreateElement("workOrderList");
            xelen.AppendChild(xel);
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(WorkOrder.Trim());
                XmlNodeList xnl = doc.GetElementsByTagName("request");
                foreach (XmlNode node in xnl)
                {
                    workOrderId = node["workOrderId"].FirstChild.Value;
                }

                SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon.ConnectionString = ConServerStr;
                sqlCon.Open();
                string sql = "select * from ResourceDataCollectionWorkOrder where workOrderId='" + workOrderId + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                cmd.ExecuteNonQuery();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    XmlElement Sub1 = xdoc.CreateElement("vendorID");
                    Sub1.InnerText = reader["vendorID"].ToString();
                    xel.AppendChild(Sub1);
                    XmlElement Sub2 = xdoc.CreateElement("deviceID");
                    Sub2.InnerText = reader["deviceID"].ToString();
                    xel.AppendChild(Sub2);
                }
                reader.Close();               
                cmd.Dispose();
                sqlCon.Dispose();

                SqlConnection sqlCon2 = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon2.ConnectionString = ConServerStr;
                sqlCon2.Open();
                string sql1 = "select * from resorceData where workOrderId='" + workOrderId + "'";
                SqlCommand cmd1 = new SqlCommand(sql1, sqlCon2);
                cmd1.ExecuteNonQuery();
                SqlDataReader reader1 = cmd1.ExecuteReader();
                while (reader1.Read())
                {
                    XmlElement xel2 = xdoc.CreateElement("wElectronicIdInfo");
                    xel.AppendChild(xel2);
                    XmlElement Sub1 = xdoc.CreateElement("frameNo");
                    Sub1.InnerText = reader1["frameNo"].ToString();
                    xel2.AppendChild(Sub1);
                    XmlElement Sub2 = xdoc.CreateElement("boardNo");
                    Sub2.InnerText = reader1["boardNo"].ToString();
                    xel2.AppendChild(Sub2);
                    XmlElement Sub3 = xdoc.CreateElement("portNo");
                    Sub3.InnerText = reader1["portNo"].ToString();
                    xel2.AppendChild(Sub3);
                    XmlElement Sub4 = xdoc.CreateElement("electronicIdInfo");
                    Sub4.InnerText = reader1["electronicIdInfo"].ToString();
                    xel2.AppendChild(Sub4);
                }
                reader1.Close();                
                cmd1.Dispose();
                sqlCon2.Dispose();
                
                //删除相应工单内容
                string sql5 = "delete from ResourceDataCollectionWorkOrder where workOrderId='" + workOrderId + "'";
                SqlCommand cmd5 = new SqlCommand(sql5, sqlCon1);
                cmd5.ExecuteNonQuery();
                cmd5.Dispose();

                string sql6 = "delete from resorceData where workOrderId='" + workOrderId + "'";
                SqlCommand cmd6 = new SqlCommand(sql6, sqlCon1);
                cmd6.ExecuteNonQuery();
                cmd6.Dispose();
               
            }
            catch (Exception e)
            {
                XmlElement xSub31 = xdoc.CreateElement("result");
//                xSub31.InnerText = Convert.ToString(e);
                xSub31.InnerText = "false";
                xelen.AppendChild(xSub31);
                return xdoc.InnerXml;
            }
            return xdoc.InnerXml;
        }
		
		
		        /// <summary>
        /// PC上传电子标签写入工单
        /// </summary>
        /// <param name="ElectricalIDWritingWorkOrder"></param>
        /// <returns></returns>
        public string PCuploadElectricalIDWritingWorkOrder(string ElectricalIDWritingWorkOrder)
        {
            string workOrderId = "";
            string vendorID = "";
            string deviceID = "";

            string frameNo = "";
            string boardNo = "";
            string portNo = "";
            string electronicIdInfo = "";

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(ElectricalIDWritingWorkOrder.Trim());
                XmlNodeList xnl = doc.GetElementsByTagName("workOrderList");
                foreach (XmlNode node in xnl)
                {
                    XmlElement xe = (XmlElement)node;
                    workOrderId = xe.SelectSingleNode("workOrderId").InnerText.Trim();
                    vendorID = xe.SelectSingleNode("vendorID").InnerText.Trim();
                    deviceID = xe.SelectSingleNode("deviceID").InnerText.Trim();

                    SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                    sqlCon.ConnectionString = ConServerStr;
                    sqlCon.Open();
                    string sql = "insert into ResourceDataCollectionWorkOrder (vendorID,deviceID,workOrderId)"
                                + "values ('" + vendorID + "','" + deviceID + "','" + workOrderId + "')";
                    SqlCommand cmd = new SqlCommand(sql, sqlCon1);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    sqlCon.Dispose();
                }
                XmlNodeList xnl2 = doc.GetElementsByTagName("wElectronicIdInfo");
                foreach (XmlNode node in xnl2)
                {
                    XmlElement xe = (XmlElement)node;
                    frameNo = xe.SelectSingleNode("frameNo").InnerText.Trim();
                    boardNo = xe.SelectSingleNode("boardNo").InnerText.Trim();
                    portNo = xe.SelectSingleNode("portNo").InnerText.Trim();
                    electronicIdInfo = xe.SelectSingleNode("electronicIdInfo").InnerText.Trim();

                    SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                    sqlCon.ConnectionString = ConServerStr;
                    sqlCon.Open();
                    string sql2 = "insert into resorceData (frameNo,boardNo,portNo,electronicIdInfo,workOrderId)"
                                + "values ('" + frameNo + "','" + boardNo + "','" + portNo + "','" + electronicIdInfo + "','" + workOrderId + "')";
                    SqlCommand cmd2 = new SqlCommand(sql2, sqlCon1);
                    cmd2.ExecuteNonQuery();
                    cmd2.Dispose();
                    sqlCon.Dispose();
                }
            }
            catch (Exception e)
            {
//                return Convert.ToString(e);
                return "false";
            }
            return "true";
        }



        /// <summary>
        /// 6.14上传资源数据信息
        /// </summary>
        /// <param name="Resourcedata"></param>
        /// <returns></returns>
        public string uploadResourcedata(string Resourcedata)
        {
            string deviceId = "";
            string deviceType = "";
            string vendorId = "";
            string vendorDeviceType = "";
            string frameNo = "";
            string boardNo = "";
            string portNo = "";
            string electronicIdInfo = "";

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Resourcedata.Trim());
                XmlNodeList xnl1 = doc.GetElementsByTagName("device");
                foreach (XmlNode node in xnl1)
                {
                    XmlElement xe = (XmlElement)node;
                    deviceId = xe.Attributes["deviceId"].Value;
                    deviceType = xe.Attributes["deviceType"].Value;
                    vendorId = xe.Attributes["vendorId"].Value;
                    vendorDeviceType = xe.Attributes["vendorDeviceType"].Value;

                    SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                    sqlCon.ConnectionString = ConServerStr;
                    sqlCon.Open();
                    string sql = "insert into device_data (deviceId,deviceType,vendorId,vendorDeviceType)"
                                + "values ('" + deviceId + "','" + deviceType + "','" + vendorId + "','" + vendorDeviceType + "')";
                    SqlCommand cmd = new SqlCommand(sql, sqlCon);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    sqlCon.Dispose();
                }
                XmlNodeList xnl2 = doc.GetElementsByTagName("frame");
                foreach (XmlNode node in xnl2)
                {
                    XmlElement xe = (XmlElement)node;
                    deviceId = xe.Attributes["deviceId"].Value;
                    frameNo = xe.Attributes["frameNo"].Value;

                    SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                    sqlCon.ConnectionString = ConServerStr;
                    sqlCon.Open();
                    string sql = "insert into frame_data (deviceId,frameNo)"
                                + "values ('" + deviceId + "','" + frameNo + "')";
                    SqlCommand cmd = new SqlCommand(sql, sqlCon);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    sqlCon.Dispose();
                }
                XmlNodeList xnl3 = doc.GetElementsByTagName("board");
                foreach (XmlNode node in xnl3)
                {
                    XmlElement xe = (XmlElement)node;
                    deviceId = xe.Attributes["deviceId"].Value;
                    frameNo = xe.Attributes["frameNo"].Value;
                    boardNo = xe.Attributes["boardNo"].Value;

                    SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                    sqlCon.ConnectionString = ConServerStr;
                    sqlCon.Open();
                    string sql = "insert into board_data (deviceId,frameNo,boardNo)"
                                + "values ('" + deviceId + "','" + frameNo + "','" + boardNo + "')";
                    SqlCommand cmd = new SqlCommand(sql, sqlCon);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    sqlCon.Dispose();
                }
                XmlNodeList xnl4 = doc.GetElementsByTagName("port");
                foreach (XmlNode node in xnl4)
                {
                    XmlElement xe = (XmlElement)node;
                    deviceId = xe.Attributes["deviceId"].Value;
                    frameNo = xe.Attributes["frameNo"].Value;
                    boardNo = xe.Attributes["boardNo"].Value;
                    portNo = xe.Attributes["portNo"].Value;
                    electronicIdInfo = xe.Attributes["electronicIdInfo"].Value;

                    SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                    sqlCon.ConnectionString = ConServerStr;
                    sqlCon.Open();
                    string sql = "insert into port_data (deviceId,frameNo,boardNo,portNo,electronicIdInfo)"
                                + "values ('" + deviceId + "','" + frameNo + "','" + boardNo + "','" + portNo + "','" + electronicIdInfo + "')";
                    SqlCommand cmd = new SqlCommand(sql, sqlCon);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    sqlCon.Dispose();
                }
            }
            catch (Exception e)
            {
                XmlDocument xdoc2 = new XmlDocument();
                XmlDeclaration xdec2 = xdoc2.CreateXmlDeclaration("1.0", "UTF-8", null);
                xdoc2.AppendChild(xdec2);
                XmlElement xelen2 = xdoc2.CreateElement("response");
                xdoc2.AppendChild(xelen2);
                XmlElement Sub3 = xdoc2.CreateElement("result");
                //                Sub3.InnerText = "false"+Convert.ToString(e);
                Sub3.InnerText = "false";
                xelen2.AppendChild(Sub3);
                return xdoc2.InnerXml;
            }
            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdecl = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xdoc.AppendChild(xdecl);
            XmlElement xelen = xdoc.CreateElement("response");
            xdoc.AppendChild(xelen);
            XmlElement Sub4 = xdoc.CreateElement("result");
            Sub4.InnerText = "true";
            xelen.AppendChild(Sub4);
            return xdoc.InnerXml;
        }

        /// <summary>
        /// 6.14上传资源数据信息
        /// </summary>
        /// <param name="Resourcedata"></param>
        /// <returns></returns>
        public string uploadResourcedata(string Resourcedata)
        {
            string deviceId = "";
            string deviceType = "";
            string vendorId = "";
            string vendorDeviceType = "";
            string frameNo = "";
            string boardNo = "";
            string portNo = "";
            string electronicIdInfo = "";

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Resourcedata.Trim());
                XmlNodeList xnl1 = doc.GetElementsByTagName("device");
                foreach (XmlNode node in xnl1)
                {
                    XmlElement xe = (XmlElement)node;
                    deviceId = xe.Attributes["deviceId"].Value;
                    deviceType = xe.Attributes["deviceType"].Value;
                    vendorId = xe.Attributes["vendorId"].Value;
                    vendorDeviceType = xe.Attributes["vendorDeviceType"].Value;

                    SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                    sqlCon.ConnectionString = ConServerStr;
                    sqlCon.Open();
                    string sql = "insert into device_data (deviceId,deviceType,vendorId,vendorDeviceType)"
                                + "values ('" + deviceId + "','" + deviceType + "','" + vendorId + "','" + vendorDeviceType + "')";
                    SqlCommand cmd = new SqlCommand(sql, sqlCon);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    sqlCon.Dispose();
                }
                XmlNodeList xnl2 = doc.GetElementsByTagName("frame");
                foreach (XmlNode node in xnl2)
                {
                    XmlElement xe = (XmlElement)node;
                    deviceId = xe.Attributes["deviceId"].Value;
                    frameNo = xe.Attributes["frameNo"].Value;

                    SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                    sqlCon.ConnectionString = ConServerStr;
                    sqlCon.Open();
                    string sql = "insert into frame_data (deviceId,frameNo)"
                                + "values ('" + deviceId + "','" + frameNo + "')";
                    SqlCommand cmd = new SqlCommand(sql, sqlCon);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    sqlCon.Dispose();
                }
                XmlNodeList xnl3 = doc.GetElementsByTagName("board");
                foreach (XmlNode node in xnl3)
                {
                    XmlElement xe = (XmlElement)node;
                    deviceId = xe.Attributes["deviceId"].Value;
                    frameNo = xe.Attributes["frameNo"].Value;
                    boardNo = xe.Attributes["boardNo"].Value;

                    SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                    sqlCon.ConnectionString = ConServerStr;
                    sqlCon.Open();
                    string sql = "insert into board_data (deviceId,frameNo,boardNo)"
                                + "values ('" + deviceId + "','" + frameNo + "','" + boardNo + "')";
                    SqlCommand cmd = new SqlCommand(sql, sqlCon);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    sqlCon.Dispose();
                }
                XmlNodeList xnl4 = doc.GetElementsByTagName("port");
                foreach (XmlNode node in xnl4)
                {
                    XmlElement xe = (XmlElement)node;
                    deviceId = xe.Attributes["deviceId"].Value;
                    frameNo = xe.Attributes["frameNo"].Value;
                    boardNo = xe.Attributes["boardNo"].Value;
                    portNo = xe.Attributes["portNo"].Value;
                    electronicIdInfo = xe.Attributes["electronicIdInfo"].Value;

                    SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                    sqlCon.ConnectionString = ConServerStr;
                    sqlCon.Open();
                    string sql = "insert into port_data (deviceId,frameNo,boardNo,portNo,electronicIdInfo)"
                                + "values ('" + deviceId + "','" + frameNo + "','" + boardNo + "','" + portNo + "','" + electronicIdInfo + "')";
                    SqlCommand cmd = new SqlCommand(sql, sqlCon);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    sqlCon.Dispose();
                }
            }
            catch (Exception e)
            {
                XmlDocument xdoc2 = new XmlDocument();
                XmlDeclaration xdec2 = xdoc2.CreateXmlDeclaration("1.0", "UTF-8", null);
                xdoc2.AppendChild(xdec2);
                XmlElement xelen2 = xdoc2.CreateElement("response");
                xdoc2.AppendChild(xelen2);
                XmlElement Sub3 = xdoc2.CreateElement("result");
                //                Sub3.InnerText = "false"+Convert.ToString(e);
                Sub3.InnerText = "false";
                xelen2.AppendChild(Sub3);
                return xdoc2.InnerXml;
            }
            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdecl = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xdoc.AppendChild(xdecl);
            XmlElement xelen = xdoc.CreateElement("response");
            xdoc.AppendChild(xelen);
            XmlElement Sub4 = xdoc.CreateElement("result");
            Sub4.InnerText = "true";
            xelen.AppendChild(Sub4);
            return xdoc.InnerXml;
        }

        /// <summary>
        /// PC下载资源数据信息（传入参数为设备ID）
        /// </summary>
        /// <returns></returns>
        public string PCDownLoadResourcedata(string deviceId)
        {
            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdecl = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xdoc.AppendChild(xdecl);
            XmlElement xelen = xdoc.CreateElement("response");
            xdoc.AppendChild(xelen);
            XmlElement xelen1 = xdoc.CreateElement("inODNResource");
            xelen.AppendChild(xelen1);

            try
            {
                XmlElement xel1 = xdoc.CreateElement("devices");
                xelen1.AppendChild(xel1);

                SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon.ConnectionString = ConServerStr;
                sqlCon.Open();
                string sql1 = "select * from device_data where deviceId = '" + deviceId + "'";
                SqlCommand cmd1 = new SqlCommand(sql1, sqlCon);
                cmd1.ExecuteNonQuery();
                SqlDataReader reader1 = cmd1.ExecuteReader();
                while (reader1.Read())
                {
                    XmlElement Sub = xdoc.CreateElement("device");
                    xel1.AppendChild(Sub);
                    XmlAttribute suttitle1 = xdoc.CreateAttribute("deviceId");
                    suttitle1.Value = reader1["deviceId"].ToString();
                    Sub.Attributes.Append(suttitle1);
                    XmlAttribute suttitle2 = xdoc.CreateAttribute("deviceType");
                    suttitle2.Value = reader1["deviceType"].ToString();
                    Sub.Attributes.Append(suttitle2);
                    XmlAttribute suttitle3 = xdoc.CreateAttribute("vendorId");
                    suttitle3.Value = reader1["vendorId"].ToString();
                    Sub.Attributes.Append(suttitle3);
                    XmlAttribute suttitle4 = xdoc.CreateAttribute("vendorDeviceType");
                    suttitle4.Value = reader1["vendorDeviceType"].ToString();
                    Sub.Attributes.Append(suttitle4);
                }
                reader1.Close();
                cmd1.Dispose();
                sqlCon.Dispose();

                XmlElement xel2 = xdoc.CreateElement("framess");
                xelen1.AppendChild(xel2);

                SqlConnection sqlCon2 = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon2.ConnectionString = ConServerStr;
                sqlCon2.Open();
                string sql2 = "select * from frame_data where deviceId = '" + deviceId + "'";
                SqlCommand cmd2 = new SqlCommand(sql2, sqlCon2);
                cmd2.ExecuteNonQuery();
                SqlDataReader reader2 = cmd2.ExecuteReader();
                while (reader2.Read())
                {
                    XmlElement Sub = xdoc.CreateElement("frame");
                    xel2.AppendChild(Sub);
                    XmlAttribute suttitle1 = xdoc.CreateAttribute("deviceId");
                    suttitle1.Value = reader2["deviceId"].ToString();
                    Sub.Attributes.Append(suttitle1);
                    XmlAttribute suttitle2 = xdoc.CreateAttribute("frameNo");
                    suttitle2.Value = reader2["frameNo"].ToString();
                    Sub.Attributes.Append(suttitle2);
                }
                reader2.Close();
                cmd2.Dispose();
                sqlCon2.Dispose();

                XmlElement xel3 = xdoc.CreateElement("boards");
                xelen1.AppendChild(xel3);

                SqlConnection sqlCon3 = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon3.ConnectionString = ConServerStr;
                sqlCon3.Open();
                string sql3 = "select * from board_data where deviceId = '" + deviceId + "'";
                SqlCommand cmd3 = new SqlCommand(sql3, sqlCon3);
                cmd3.ExecuteNonQuery();
                SqlDataReader reader3 = cmd3.ExecuteReader();
                while (reader3.Read())
                {
                    XmlElement Sub = xdoc.CreateElement("board");
                    xel3.AppendChild(Sub);
                    XmlAttribute suttitle1 = xdoc.CreateAttribute("deviceId");
                    suttitle1.Value = reader3["deviceId"].ToString();
                    Sub.Attributes.Append(suttitle1);
                    XmlAttribute suttitle2 = xdoc.CreateAttribute("frameNo");
                    suttitle2.Value = reader3["frameNo"].ToString();
                    Sub.Attributes.Append(suttitle2);
                    XmlAttribute suttitle3 = xdoc.CreateAttribute("boardNo");
                    suttitle3.Value = reader3["boardNo"].ToString();
                    Sub.Attributes.Append(suttitle3);
                }
                reader3.Close();
                cmd3.Dispose();
                sqlCon3.Dispose();

                XmlElement xel4 = xdoc.CreateElement("ports");
                xelen1.AppendChild(xel4);

                SqlConnection sqlCon4 = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon4.ConnectionString = ConServerStr;
                sqlCon4.Open();
                string sql4 = "select * from port_data where deviceId = '" + deviceId + "'";
                SqlCommand cmd4 = new SqlCommand(sql4, sqlCon4);
                cmd4.ExecuteNonQuery();
                SqlDataReader reader4 = cmd4.ExecuteReader();
                while (reader4.Read())
                {
                    XmlElement Sub = xdoc.CreateElement("port");
                    xel4.AppendChild(Sub);
                    XmlAttribute suttitle1 = xdoc.CreateAttribute("deviceId");
                    suttitle1.Value = reader4["deviceId"].ToString();
                    Sub.Attributes.Append(suttitle1);
                    XmlAttribute suttitle2 = xdoc.CreateAttribute("frameNo");
                    suttitle2.Value = reader4["frameNo"].ToString();
                    Sub.Attributes.Append(suttitle2);
                    XmlAttribute suttitle3 = xdoc.CreateAttribute("boardNo");
                    suttitle3.Value = reader4["boardNo"].ToString();
                    Sub.Attributes.Append(suttitle3);
                    XmlAttribute suttitle4 = xdoc.CreateAttribute("portNo");
                    suttitle4.Value = reader4["portNo"].ToString();
                    Sub.Attributes.Append(suttitle4);
                    XmlAttribute suttitle5 = xdoc.CreateAttribute("electronicIdInfo");
                    suttitle5.Value = reader4["electronicIdInfo"].ToString();
                    Sub.Attributes.Append(suttitle5);
                }
                reader4.Close();
                cmd4.Dispose();
                sqlCon4.Dispose();

                //删除相应工单内容
                string sql5 = "delete from device_data where deviceId = '" + deviceId + "'";
                SqlCommand cmd5 = new SqlCommand(sql5, sqlCon1);
                cmd5.ExecuteNonQuery();
                cmd5.Dispose();

                string sql6 = "delete from frame_data where deviceId = '" + deviceId + "'";
                SqlCommand cmd6 = new SqlCommand(sql6, sqlCon1);
                cmd6.ExecuteNonQuery();
                cmd6.Dispose();

                string sql7 = "delete from board_data where deviceId = '" + deviceId + "'";
                SqlCommand cmd7 = new SqlCommand(sql7, sqlCon1);
                cmd7.ExecuteNonQuery();
                cmd7.Dispose();

                string sql8 = "delete from port_data where deviceId = '" + deviceId + "'";
                SqlCommand cmd8 = new SqlCommand(sql8, sqlCon1);
                cmd8.ExecuteNonQuery();
                cmd8.Dispose();
            }
            catch (Exception e)
            {
                XmlElement xSub31 = xdoc.CreateElement("result");
//                xSub31.InnerText = Convert.ToString(e);
                xSub31.InnerText = "false";
                xelen.AppendChild(xSub31);
                return xdoc.InnerXml;
            }
            return xdoc.InnerXml;
        }



        /// <summary>
        /// 6.15下载资源数据信息
        /// </summary>
        /// <param name="Resourcedata"></param>
        /// <returns></returns>
        public string dowmloadResourcedata(string Resourcedata)
        {
            string deviceId = "";

            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdecl = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xdoc.AppendChild(xdecl);
            XmlElement xelen = xdoc.CreateElement("response");
            xdoc.AppendChild(xelen);
            XmlElement xel = xdoc.CreateElement("inODNResource");
            xelen.AppendChild(xel);
            try
            {
                XmlDocument doc = new XmlDocument();

                doc.LoadXml(Resourcedata.Trim());
                XmlNodeList xnl = doc.GetElementsByTagName("request");
                foreach (XmlNode node in xnl)
                {
                    deviceId = node["deviceId"].FirstChild.Value;
                }
                XmlElement xel1 = xdoc.CreateElement("devices");
                xel.AppendChild(xel1);

                SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon.ConnectionString = ConServerStr;
                sqlCon.Open();
                string sql1 = "select * from device_data where workID='" + deviceId + "'";
                SqlCommand cmd1 = new SqlCommand(sql1, sqlCon);
                cmd1.ExecuteNonQuery();
                SqlDataReader reader = cmd1.ExecuteReader();
                while (reader.Read())
                {
                    deviceId = reader["deviceId"].ToString();
                    XmlElement Sub1 = xdoc.CreateElement("device");
                    xel1.AppendChild(Sub1);
                    XmlAttribute suttitle11 = xdoc.CreateAttribute("deviceId");
                    suttitle11.Value = reader["deviceId"].ToString();
                    Sub1.Attributes.Append(suttitle11);
                    XmlAttribute suttitle12 = xdoc.CreateAttribute("deviceType");
                    suttitle12.Value = reader["deviceType"].ToString();
                    Sub1.Attributes.Append(suttitle12);
                    XmlAttribute suttitle13 = xdoc.CreateAttribute("vendorId");
                    suttitle13.Value = reader["vendorId"].ToString();
                    Sub1.Attributes.Append(suttitle13);
                    XmlAttribute suttitle14 = xdoc.CreateAttribute("vendorDeviceType");
                    suttitle14.Value = reader["vendorDeviceType"].ToString();
                    Sub1.Attributes.Append(suttitle14);
                }
                reader.Close();
                cmd1.Dispose();
                sqlCon.Dispose();

                XmlElement xel2 = xdoc.CreateElement("frames");
                xel.AppendChild(xel2);

                SqlConnection sqlCon2 = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon2.ConnectionString = ConServerStr;
                sqlCon2.Open();
                string sql2 = "select * from frame_data where deviceId='" + deviceId + "'";
                SqlCommand cmd2 = new SqlCommand(sql2, sqlCon2);
                cmd2.ExecuteNonQuery();
                SqlDataReader reader2 = cmd2.ExecuteReader();
                while (reader2.Read())
                {
                    XmlElement Sub2 = xdoc.CreateElement("frame");
                    xel2.AppendChild(Sub2);
                    XmlAttribute suttitle11 = xdoc.CreateAttribute("deviceId");
                    suttitle11.Value = reader2["deviceId"].ToString();
                    Sub2.Attributes.Append(suttitle11);
                    XmlAttribute suttitle12 = xdoc.CreateAttribute("frameNo");
                    suttitle12.Value = reader2["frameNo"].ToString();
                    Sub2.Attributes.Append(suttitle12);
                }
                reader2.Close();
                cmd2.Dispose();
                sqlCon2.Dispose();

                XmlElement xel3 = xdoc.CreateElement("boards");
                xel.AppendChild(xel3);

                SqlConnection sqlCon3 = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon3.ConnectionString = ConServerStr;
                sqlCon3.Open();
                string sql3 = "select * from board_data where deviceId='" + deviceId + "'";
                SqlCommand cmd3 = new SqlCommand(sql3, sqlCon3);
                cmd3.ExecuteNonQuery();
                SqlDataReader reader3 = cmd3.ExecuteReader();
                while (reader3.Read())
                {
                    XmlElement Sub3 = xdoc.CreateElement("board");
                    xel3.AppendChild(Sub3);
                    XmlAttribute suttitle11 = xdoc.CreateAttribute("deviceId");
                    suttitle11.Value = reader3["deviceId"].ToString();
                    Sub3.Attributes.Append(suttitle11);
                    XmlAttribute suttitle12 = xdoc.CreateAttribute("frameNo");
                    suttitle12.Value = reader3["frameNo"].ToString();
                    Sub3.Attributes.Append(suttitle12);
                    XmlAttribute suttitle13 = xdoc.CreateAttribute("boardNo");
                    suttitle13.Value = reader3["boardNo"].ToString();
                    Sub3.Attributes.Append(suttitle13);
                }
                reader3.Close();
                cmd3.Dispose();
                sqlCon3.Dispose();

                XmlElement xel4 = xdoc.CreateElement("ports");
                xel.AppendChild(xel4);

                SqlConnection sqlCon4 = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon4.ConnectionString = ConServerStr;
                sqlCon4.Open();
                string sql4 = "select * from port_data where deviceId='" + deviceId + "'";
                SqlCommand cmd4 = new SqlCommand(sql4, sqlCon4);
                cmd4.ExecuteNonQuery();
                SqlDataReader reader4 = cmd4.ExecuteReader();
                while (reader4.Read())
                {
                    XmlElement Sub4 = xdoc.CreateElement("port");
                    xel4.AppendChild(Sub4);
                    XmlAttribute suttitle11 = xdoc.CreateAttribute("deviceId");
                    suttitle11.Value = reader4["deviceId"].ToString();
                    Sub4.Attributes.Append(suttitle11);
                    XmlAttribute suttitle12 = xdoc.CreateAttribute("frameNo");
                    suttitle12.Value = reader4["frameNo"].ToString();
                    Sub4.Attributes.Append(suttitle12);
                    XmlAttribute suttitle13 = xdoc.CreateAttribute("boardNo");
                    suttitle13.Value = reader4["boardNo"].ToString();
                    Sub4.Attributes.Append(suttitle13);
                    XmlAttribute suttitle14 = xdoc.CreateAttribute("portNo");
                    suttitle14.Value = reader4["portNo"].ToString();
                    Sub4.Attributes.Append(suttitle14);
                    XmlAttribute suttitle15 = xdoc.CreateAttribute("electronicIdInfo");
                    suttitle15.Value = reader4["electronicIdInfo"].ToString();
                    Sub4.Attributes.Append(suttitle15);
                }
                reader4.Close();
                cmd4.Dispose();
                sqlCon4.Dispose();


                //删除相应工单内容
                string sql5 = "delete from device_data where deviceId='" + deviceId + "'";
                SqlCommand cmd5 = new SqlCommand(sql5, sqlCon1);
                cmd5.ExecuteNonQuery();
                cmd5.Dispose();

                string sql6 = "delete from frame_data where deviceId='" + deviceId + "'";
                SqlCommand cmd6 = new SqlCommand(sql6, sqlCon1);
                cmd6.ExecuteNonQuery();
                cmd6.Dispose();

                string sql7 = "delete from board_data where deviceId='" + deviceId + "'";
                SqlCommand cmd7 = new SqlCommand(sql7, sqlCon1);
                cmd7.ExecuteNonQuery();
                cmd7.Dispose();

                string sql8 = "delete from port_data where deviceId='" + deviceId + "'";
                SqlCommand cmd8 = new SqlCommand(sql8, sqlCon1);
                cmd8.ExecuteNonQuery();
                cmd8.Dispose();
            }
            catch (Exception e)
            {
                XmlElement Sub3 = xdoc.CreateElement("result");
                //                Sub3.InnerText = "false" + Convert.ToString(e);
                Sub3.InnerText = "false";
                xel.AppendChild(Sub3);
                return xdoc.InnerXml;
            }
            return xdoc.InnerXml;
        }



        /// <summary>
        /// 6.15下载资源数据信息
        /// </summary>
        /// <param name="Resourcedata"></param>
        /// <returns></returns>
        public string downloadResourcedata(string Resourcedata)
        {
            string deviceId = "";

            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xdecl = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xdoc.AppendChild(xdecl);
            XmlElement xelen = xdoc.CreateElement("response");
            xdoc.AppendChild(xelen);
            XmlElement xel = xdoc.CreateElement("inODNResource");
            xelen.AppendChild(xel);
            try
            {
                XmlDocument doc = new XmlDocument();

                doc.LoadXml(Resourcedata.Trim());
                XmlNodeList xnl = doc.GetElementsByTagName("request");
                foreach (XmlNode node in xnl)
                {
                    deviceId = node["deviceId"].FirstChild.Value;
                }
                XmlElement xel1 = xdoc.CreateElement("devices");
                xel.AppendChild(xel1);

                SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon.ConnectionString = ConServerStr;
                sqlCon.Open();
                string sql1 = "select * from device_data where workID='" + deviceId + "'";
                SqlCommand cmd1 = new SqlCommand(sql1, sqlCon);
                cmd1.ExecuteNonQuery();
                SqlDataReader reader = cmd1.ExecuteReader();
                while (reader.Read())
                {
                    deviceId = reader["deviceId"].ToString();
                    XmlElement Sub1 = xdoc.CreateElement("device");
                    xel1.AppendChild(Sub1);
                    XmlAttribute suttitle11 = xdoc.CreateAttribute("deviceId");
                    suttitle11.Value = reader["deviceId"].ToString();
                    Sub1.Attributes.Append(suttitle11);
                    XmlAttribute suttitle12 = xdoc.CreateAttribute("deviceType");
                    suttitle12.Value = reader["deviceType"].ToString();
                    Sub1.Attributes.Append(suttitle12);
                    XmlAttribute suttitle13 = xdoc.CreateAttribute("vendorId");
                    suttitle13.Value = reader["vendorId"].ToString();
                    Sub1.Attributes.Append(suttitle13);
                    XmlAttribute suttitle14 = xdoc.CreateAttribute("vendorDeviceType");
                    suttitle14.Value = reader["vendorDeviceType"].ToString();
                    Sub1.Attributes.Append(suttitle14);
                }
                reader.Close();
                cmd1.Dispose();
                sqlCon.Dispose();

                XmlElement xel2 = xdoc.CreateElement("frames");
                xel.AppendChild(xel2);

                SqlConnection sqlCon2 = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon2.ConnectionString = ConServerStr;
                sqlCon2.Open();
                string sql2 = "select * from frame_data where deviceId='" + deviceId + "'";
                SqlCommand cmd2 = new SqlCommand(sql2, sqlCon2);
                cmd2.ExecuteNonQuery();
                SqlDataReader reader2 = cmd2.ExecuteReader();
                while (reader2.Read())
                {
                    XmlElement Sub2 = xdoc.CreateElement("frame");
                    xel2.AppendChild(Sub2);
                    XmlAttribute suttitle11 = xdoc.CreateAttribute("deviceId");
                    suttitle11.Value = reader2["deviceId"].ToString();
                    Sub2.Attributes.Append(suttitle11);
                    XmlAttribute suttitle12 = xdoc.CreateAttribute("frameNo");
                    suttitle12.Value = reader2["frameNo"].ToString();
                    Sub2.Attributes.Append(suttitle12);
                }
                reader2.Close();
                cmd2.Dispose();
                sqlCon2.Dispose();

                XmlElement xel3 = xdoc.CreateElement("boards");
                xel.AppendChild(xel3);

                SqlConnection sqlCon3 = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon3.ConnectionString = ConServerStr;
                sqlCon3.Open();
                string sql3 = "select * from board_data where deviceId='" + deviceId + "'";
                SqlCommand cmd3 = new SqlCommand(sql3, sqlCon3);
                cmd3.ExecuteNonQuery();
                SqlDataReader reader3 = cmd3.ExecuteReader();
                while (reader3.Read())
                {
                    XmlElement Sub3 = xdoc.CreateElement("board");
                    xel3.AppendChild(Sub3);
                    XmlAttribute suttitle11 = xdoc.CreateAttribute("deviceId");
                    suttitle11.Value = reader3["deviceId"].ToString();
                    Sub3.Attributes.Append(suttitle11);
                    XmlAttribute suttitle12 = xdoc.CreateAttribute("frameNo");
                    suttitle12.Value = reader3["frameNo"].ToString();
                    Sub3.Attributes.Append(suttitle12);
                    XmlAttribute suttitle13 = xdoc.CreateAttribute("boardNo");
                    suttitle13.Value = reader3["boardNo"].ToString();
                    Sub3.Attributes.Append(suttitle13);
                }
                reader3.Close();
                cmd3.Dispose();
                sqlCon3.Dispose();

                XmlElement xel4 = xdoc.CreateElement("ports");
                xel.AppendChild(xel4);

                SqlConnection sqlCon4 = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                sqlCon4.ConnectionString = ConServerStr;
                sqlCon4.Open();
                string sql4 = "select * from port_data where deviceId='" + deviceId + "'";
                SqlCommand cmd4 = new SqlCommand(sql4, sqlCon4);
                cmd4.ExecuteNonQuery();
                SqlDataReader reader4 = cmd4.ExecuteReader();
                while (reader4.Read())
                {
                    XmlElement Sub4 = xdoc.CreateElement("port");
                    xel4.AppendChild(Sub4);
                    XmlAttribute suttitle11 = xdoc.CreateAttribute("deviceId");
                    suttitle11.Value = reader4["deviceId"].ToString();
                    Sub4.Attributes.Append(suttitle11);
                    XmlAttribute suttitle12 = xdoc.CreateAttribute("frameNo");
                    suttitle12.Value = reader4["frameNo"].ToString();
                    Sub4.Attributes.Append(suttitle12);
                    XmlAttribute suttitle13 = xdoc.CreateAttribute("boardNo");
                    suttitle13.Value = reader4["boardNo"].ToString();
                    Sub4.Attributes.Append(suttitle13);
                    XmlAttribute suttitle14 = xdoc.CreateAttribute("portNo");
                    suttitle14.Value = reader4["portNo"].ToString();
                    Sub4.Attributes.Append(suttitle14);
                    XmlAttribute suttitle15 = xdoc.CreateAttribute("electronicIdInfo");
                    suttitle15.Value = reader4["electronicIdInfo"].ToString();
                    Sub4.Attributes.Append(suttitle15);
                }
                reader4.Close();
                cmd4.Dispose();
                sqlCon4.Dispose();


                //删除相应工单内容
                string sql5 = "delete from device_data where deviceId='" + deviceId + "'";
                SqlCommand cmd5 = new SqlCommand(sql5, sqlCon1);
                cmd5.ExecuteNonQuery();
                cmd5.Dispose();

                string sql6 = "delete from frame_data where deviceId='" + deviceId + "'";
                SqlCommand cmd6 = new SqlCommand(sql6, sqlCon1);
                cmd6.ExecuteNonQuery();
                cmd6.Dispose();

                string sql7 = "delete from board_data where deviceId='" + deviceId + "'";
                SqlCommand cmd7 = new SqlCommand(sql7, sqlCon1);
                cmd7.ExecuteNonQuery();
                cmd7.Dispose();

                string sql8 = "delete from port_data where deviceId='" + deviceId + "'";
                SqlCommand cmd8 = new SqlCommand(sql8, sqlCon1);
                cmd8.ExecuteNonQuery();
                cmd8.Dispose();
            }
            catch (Exception e)
            {
                XmlElement Sub3 = xdoc.CreateElement("result");
                //                Sub3.InnerText = "false" + Convert.ToString(e);
                Sub3.InnerText = "false";
                xel.AppendChild(Sub3);
                return xdoc.InnerXml;
            }
            return xdoc.InnerXml;
        }


        /// <summary>
        /// PC上传资源下载工单信息
        /// </summary>
        /// <param name="Resourcedata"></param>
        /// <returns></returns>
        public string PCuploadResourcedata(string Resourcedata)
        {
            string deviceId = "";
            string deviceType = "";
            string vendorId = "";
            string vendorDeviceType = "";
            string workID = "";
            string frameNo = "";
            string boardNo = "";
            string portNo = "";
            string electronicIdInfo = "";
            try
            {
                XmlDocument adoc = new XmlDocument();
                adoc.LoadXml(Resourcedata.Trim());

                XmlNodeList xnl1 = adoc.GetElementsByTagName("device");
                if (xnl1.Count != 0)
                {
                    foreach (XmlNode node1 in xnl1)
                    {
                        deviceId = node1.Attributes["deviceId"].Value;
                        deviceType = node1.Attributes["deviceType"].Value;
                        vendorId = node1.Attributes["vendorId"].Value;
                        vendorDeviceType = node1.Attributes["vendorDeviceType"].Value;
                        workID = node1.Attributes["workID"].Value;

                        SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                        sqlCon.ConnectionString = ConServerStr;
                        sqlCon.Open();
                        string sql = "insert into device_data (deviceId,deviceType,vendorId,vendorDeviceType,workID)"
                            + "values ('" + deviceId + "','" + deviceType + "','" + vendorId + "','" + vendorDeviceType + "','" + workID + "')";
                        SqlCommand cmd = new SqlCommand(sql, sqlCon);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        sqlCon.Dispose();
                    }
                }

                XmlNodeList xnl2 = adoc.GetElementsByTagName("frame");
                if (xnl2.Count != 0)
                {
                    foreach (XmlNode node2 in xnl2)
                    {
                        deviceId = node2.Attributes["deviceId"].Value;
                        frameNo = node2.Attributes["frameNo"].Value;

                        SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                        sqlCon.ConnectionString = ConServerStr;
                        sqlCon.Open();
                        string sql = "insert into frame_data (deviceId,frameNo)"
                            + "values ('" + deviceId + "','" + frameNo + "')";
                        SqlCommand cmd = new SqlCommand(sql, sqlCon);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        sqlCon.Dispose();
                    }
                }

                XmlNodeList xnl3 = adoc.GetElementsByTagName("board");
                if (xnl3.Count != 0)
                {
                    foreach (XmlNode node3 in xnl3)
                    {
                        deviceId = node3.Attributes["deviceId"].Value;
                        frameNo = node3.Attributes["frameNo"].Value;
                        boardNo = node3.Attributes["boardNo"].Value;

                        SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                        sqlCon.ConnectionString = ConServerStr;
                        sqlCon.Open();
                        string sql = "insert into board_data (deviceId,frameNo,boardNo)"
                            + "values ('" + deviceId + "','" + frameNo + "','" + boardNo + "')";
                        SqlCommand cmd = new SqlCommand(sql, sqlCon);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        sqlCon.Dispose();
                    }
                }

                XmlNodeList xnl4 = adoc.GetElementsByTagName("port");
                if (xnl4.Count != 0)
                {
                    foreach (XmlNode node4 in xnl4)
                    {
                        deviceId = node4.Attributes["deviceId"].Value;
                        frameNo = node4.Attributes["frameNo"].Value;
                        boardNo = node4.Attributes["boardNo"].Value;
                        portNo = node4.Attributes["portNo"].Value;
                        electronicIdInfo = node4.Attributes["electronicIdInfo"].Value;

                        SqlConnection sqlCon = new SqlConnection();//SqlDataReader需独占一个数据库连接,要新建一个SqlDataReader需要关闭前一个SqlDataReader或新建一个数据库连接
                        sqlCon.ConnectionString = ConServerStr;
                        sqlCon.Open();
                        string sql = "insert into port_data (deviceId,frameNo,boardNo,portNo,electronicIdInfo )"
                            + "values ('" + deviceId + "','" + frameNo + "','" + boardNo + "','" + portNo + "','" + electronicIdInfo + "')";
                        SqlCommand cmd = new SqlCommand(sql, sqlCon);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        sqlCon.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                //return e.ToString();
                return "false";
            }
            return "true";
        }
		
    }
}