using System;
using AppCode;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using DataAccess;
using ERP.TAS;
using ERP.AMM;
using ERP.PopUp;
using CommonObjects;
using System.Data.Common;
using System.IO;
using DataAccess.DA.AMM;
using ERP.Security1;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Reporting.WebForms;
using DataAccess.DA.Email;

public partial class LoginAERToLogin : System.Web.UI.Page
{
    dbaccess objDB = new dbaccess();
    public string str_login_comp_code = "", str_login_role = "", str_login_user_name = "", str_login_password = "", str_is_valid = "N";
    //HttpContext.Current.Request.IsSecureConnection
    UserDatail userDetail = new UserDatail();
    string str_post_back_control = "";
     

    void CreateFolders()
    {
        var dt = objDB.execute_query_retun_datatable(@"Select PEMP_EMP_CODE from ppm_employee_details ");
        if (dt != null && dt.Rows.Count > 0)
        {
            //if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/ESS/Docs")))
            //{
            //    Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/ESS/Docs"));
            //}
            var rootPath = MapPath(@"~/Ess/Docs/");
            foreach (DataRow item in dt.Rows)
            {
                var folder = rootPath + "/" + item["PEMP_EMP_CODE"].ToString();
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);

                }
                var folder2 = folder + "/1";
                if (!Directory.Exists(folder2))
                {
                    Directory.CreateDirectory(folder2);

                }
            }
        }
    }
    [System.Web.Services.WebMethod]
    public static OtpValidationResult GenerateOTP(string userName)
    {
      
        var loginDA = new DataAccess.DA.AMM.LoginDA();
        var dt1 = loginDA.GetEmployeeDetails(userName);
        if(dt1 == null || dt1.Rows.Count == 0)
        {
            return new OtpValidationResult { Success = false, FailureReason = "Employee details are missing contact admin." };
        }
        var row1 = dt1.Rows[0];
        var result= loginDA.SendOTP(userName, row1["PEMP_EMP_NAME"].ToString(), row1["PEMP_EMAIL_ADDRESS"].ToString(), row1["PEMP_EMP_NAME"].ToString());
        if (result)
        {
            return new OtpValidationResult { Success = true, FailureReason = "OTP is sent successfully." };
        }
        else
        {
            return new OtpValidationResult { Success = false, FailureReason = "OTP is not sent." };
        }
    }
    [System.Web.Services.WebMethod]
    public static OtpValidationResult ValidateOtp(string userName, string enteredOtp, string RoleId)
    {
        var loginDA = new DataAccess.DA.AMM.LoginDA();
        var result =  loginDA.ValidateOtp(userName, enteredOtp);

        if (result.Success)
        {
            CreateSession(userName, "01", RoleId, false);
        }

        return result;

    }

    [System.Web.Services.WebMethod]
    public static UserROleDetailDTO GEtRoles(string userName)
    {

        UserROleDetailDTO dto = new UserROleDetailDTO();

        var loginDA = new DataAccess.DA.AMM.LoginDA();
        var result = loginDA.GetEmployeeDetails(userName);

        if (result.Rows.Count == 0)
        {
            dto.IsUserFound = false;
            return dto;
        }
        if (result.Rows[0]["OTP_ENABLED"].ToString() == "Y")
        {
            dto.OTPRequired = true;
        }
        dbaccess objDB = new dbaccess();
        dto.IsUserFound = true;

        var query1 = @"SELECT   URL.URL_ROLE_ID, ARD.ROLE_NAME
          FROM AMM_USER_ROLE_LNK_DETAILS URL, AMM_ROLE_DETAILS ARD, AMM_USER_DETAILS AUD 
          WHERE URL.URL_ROLE_ID = ARD.ROLE_ID 
        AND URL.URL_USER_ID = AUD.USER_ID 
    
         AND UPPER(AUD.USER_NAME) = '" + userName.ToUpper() + @"' " ;
        var dt1 = objDB.execute_query_retun_datatable(query1);
        var roles = new List<UserRoleDTO>();
        foreach (DataRow item in dt1.Rows
            )
        {
            roles.Add(new UserRoleDTO { URL_ROLE_ID = item["URL_ROLE_ID"].ToString(), ROLE_NAME = item["ROLE_NAME"].ToString(), });
        }
        dto.Roles = roles;

        return dto;


    }

    [System.Web.Services.WebMethod]
    public static OtpValidationResult ValidateUserNamePassword(string userName, string Password)
    {
        var loginDA = new DataAccess.DA.AMM.LoginDA();
        return loginDA.ValidateUserNamePassword(userName, Password);
    }
    public bool IsDevelopment = false;

    //    void automaticLogin()
    //    {PEMP_EMP_ADDRESS_3, PEMP_EMP_NKT_ADDRESS_3


    //        string str_select = @"
    //SELECT  
    //    USER_ID, USER_SHORT_NAME, USER_EMP_CODE, PEMP_EMP_CODE ,
    //    PEMP_EMP_DEPTARTMENT_CODE,PEMP_EMP_BRANCH_CODE,PBM_BRANCH_CODE, PBM_BRANCH_NAME
    //FROM 
    //    AMM_USER_DETAILS , PPM_EMPLOYEE_DETAILS  , PPM_BRANCH_MASTER 
    //where 
    //    upper(USER_NAME) = UPPER('" + Request.Cookies["L_USER"].Value + @"')
    //    AND PEMP_EMP_CODE = USER_EMP_CODE   
    //    AND USER_DEFAULT_LOCATION = PBM_BRANCH_CODE(+)
    //
    //";

    //        string str_select_func_curr_code = @"SELECT ACD_COMP_CODE, ACD_COMP_NAME,  ACD_FUNCTIONAL_CURR_CODE,MCUM_DEC_PLACE, MCUM_SUB_UNIT FROM AMM_COMPANY_DETAILS,MMM_CURRENCY_MASTER 
    //WHERE UPPER(ACD_COMP_CODE) = '" + Request.Cookies["L_COMP_CODE"].Value + @"' AND UPPER(MCUM_CURR_CODE) = UPPER(ACD_FUNCTIONAL_CURR_CODE) ";



    //        DataTable dt_result = objDB.execute_query_retun_datatable(str_select);
    //        DataTable dt_func_curr = objDB.execute_query_retun_datatable(str_select_func_curr_code);

    //        DataTable dtCompany = objDB.execute_query_retun_datatable(@"SELECT ACD_COMP_CODE, ACD_COMP_NAME FROM AMM_COMPANY_DETAILS WHERE ACD_COMP_CODE = '" + Request.Cookies["L_COMP_CODE"].Value + @"'  ");

    //        if (dtCompany != null && dtCompany.Rows.Count > 0)
    //        {
    //            userDetail.Login_Company_Code = dtCompany.Rows[0]["ACD_COMP_CODE"].ToString();
    //            userDetail.Login_Company_Name = dtCompany.Rows[0]["ACD_COMP_NAME"].ToString();

    //        }
    //        if (dt_result != null && dt_result.Rows.Count > 0)
    //        {

    //            userDetail.User_Name = Request.Cookies["L_USER"].Value  ;
    //            userDetail.User_Id = int.Parse(dt_result.Rows[0]["USER_ID"].ToString());
    //            userDetail.User_Display_Name = dt_result.Rows[0]["USER_SHORT_NAME"].ToString();
    //            userDetail.Emp_Code = dt_result.Rows[0]["USER_EMP_CODE"].ToString();
    //            userDetail.Emp_Name = dt_result.Rows[0]["PEMP_EMP_CODE"].ToString();
    //            userDetail.Role_Id = int.Parse(Request.Cookies["L_ROLE_ID"].Value);
    //            userDetail.DepartmentCode = dt_result.Rows[0]["PEMP_EMP_DEPTARTMENT_CODE"].ToString();
    //            userDetail.BranchCode = dt_result.Rows[0]["PEMP_EMP_BRANCH_CODE"].ToString();

    //            userDetail.DefaultBranchCode = dt_result.Rows[0]["PBM_BRANCH_CODE"].ToString();
    //            userDetail.DefaultBranchDesc = dt_result.Rows[0]["PBM_BRANCH_NAME"].ToString();

    //            userDetail.Role_Logo = objDB.execute_scalar(@"SELECT MCMD_ENTITY_DESC FROM AMM_ROLE_DETAILS, ACCMD_GET_COMMON_MASTER WHERE ROLE_ID = '" + userDetail.Role_Id + @"' 
    //and MCMH_ENTITY_GROUP = 'APP_LOGO'
    //and MCMD_ENTITY_CODE = ROLE_LOGO_PATH");

    //            Session["userDetail"] = userDetail;


    //        }
    //        if (dt_func_curr != null && dt_func_curr.Rows.Count > 0)
    //        {
    //            userDetail.base_curr_code = dt_func_curr.Rows[0]["ACD_FUNCTIONAL_CURR_CODE"].ToString();
    //            userDetail.base_curr_decimal_places = dt_func_curr.Rows[0]["MCUM_DEC_PLACE"].ToString();
    //            userDetail.CurrencySubUnit = dt_func_curr.Rows[0]["MCUM_SUB_UNIT"].ToString();
    //            Response.Redirect("home.aspx");
    //        }

    //    }

    public bool RunOnlyInIE
    {
        get
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["RunOnlyInIe"]))
            {
                return bool.Parse(ConfigurationManager.AppSettings["RunOnlyInIe"]);
            }
            return true;
        }
    }
    protected override void InitializeCulture()
    {
        var culture = "en-US";
        if (Session.Contents["Culture"] != null)
            culture = Session.Contents["Culture"].ToString();


        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(culture);
        System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(culture);

        base.InitializeCulture();
    }
    string GetCurrentADUserName()
    {
        try
        {
            if (!string.IsNullOrEmpty(HttpContext.Current.User.Identity.Name))
            {
                if (HttpContext.Current.User.Identity.Name.Contains("/"))
                {
                    return HttpContext.Current.User.Identity.Name.Substring(HttpContext.Current.User.Identity.Name.IndexOf("/"),
                        HttpContext.Current.User.Identity.Name.Length - HttpContext.Current.User.Identity.Name.IndexOf("/"));
                }
                else if (HttpContext.Current.User.Identity.Name.Contains(@"\"))
                {
                    return HttpContext.Current.User.Identity.Name.Substring(HttpContext.Current.User.Identity.Name.IndexOf(@"\") + 1, HttpContext.Current.User.Identity.Name.Length - HttpContext.Current.User.Identity.Name.IndexOf(@"\") - 1);
                }
            }
        }
        catch (Exception e23)
        {
            log_error.write_to_log_file("GetCurrentADUserName()", "ERRORLOG.GetCurrentADUserName()", e23.ToString());
        }
        return "";

    }

    void Teset()
    {

        DataTable table = DbProviderFactories.GetFactoryClasses();

        // Display each row and column value.
        foreach (DataRow row in table.Rows)
        {
            foreach (DataColumn column in table.Columns)
            {
                Console.WriteLine(row[column]);
            }
        }

    }

    public string pullleft = "";
    public string pulllright = "";
    public string direction = "ltr";
    public string BASE_URL_ONTC = "";
    public string BASE_URL_NFC = "";
    
    public bool IsOTPLoginEnabledYN = false;
    public bool IsNFCLogin = false;

    public string URLFirstPart = "";
    protected void Page_Load(object sender, EventArgs e)
    {

        MailFileLogger.Write(("Test"+ " Bala8...."));


        //SqlDataSource1.SelectCommand = "select user_name from amm_user_Details where rownum<10";
        //DataView dv = (DataView)SqlDataSource1.Select(DataSourceSelectArguments.Empty);
        //DataTable dt1 = dv.ToTable();

        //LogError.WriteToLogFile("", false, "dt1.Rows.Count" + Request.Url.Authority
        //    + "requested" + dt1.Rows.Count
        //    + Environment.NewLine + "Request.Url: " + Request.Url.ToString()
        //    );


        //if (Request["CF"]=="Y")
        //{
        //    CreateFolders();
        //}
        LoginDA loginDA = new LoginDA();
        var settings = loginDA.GetOtpSettings();
        IsOTPLoginEnabledYN = settings.Enabled;
        if (IsPostBack)
        {
            Form.Attributes.Add("autocomplete", "off");
        }
        // try { ada.AuthenticateActiveDirectoryAccount("Test", "1"); } catch { }
        if (HttpContext.Current.Request.IsSecureConnection)
            URLFirstPart = "https://";
        else
            URLFirstPart = "http://";

        if (Request.Url.ToString().ToLower().Contains("85.154.229.214"))
            URLFirstPart += "85.154.229.214:8443";
        else if (Request.Url.ToString().ToLower().Contains("erp.mwasalat.om"))
            URLFirstPart = "https://erp.mwasalat.om:8443";
        else
            URLFirstPart += Request.Url.Authority;

        //https://erp.mwasalat.om:8443/ONTC/LoginAER.aspx
        //http://85.154.229.214:8443/ONTC/LoginAER.aspx
        Uri uri = Request.Url;
        string requested = uri.Scheme + Uri.SchemeDelimiter + uri.Host + ":" + uri.Port;

        LogError.WriteToLogFile("", false, "Request.Url.Authority" + Request.Url.Authority
            + "requested" + requested
            + Environment.NewLine + "Request.Url: " + Request.Url.ToString()
            );

        //if (Resources.Admin.AROM == "ar-OM")
        {
            //pullleft = "pull-right";
            //pulllright = "pull-left";
            //direction = "rtl";
        }
        buttonsList.Visible = false;
        var uu = Request.Url;

        var s = Request.Url.Host;

        if (System.Configuration.ConfigurationManager.AppSettings["IS_NFC_LOGIN"] == "TRUE")
        {
            divONTC.Visible = true;
            divNormal.Visible = false;
            IsNFCLogin = true;

            ontcCompanyDDL.SelectedValue = ConfigurationManager.AppSettings["LOGIN_CODE"];
            //if (Request.Url.ToString().Contains("192.168.1.5") || Request.Url.ToString().ToLower().Contains("localhost"))
            //{
            //    if (Request.Url.ToString().ToUpper().Contains("ONTC"))
            //        Response.Redirect(ConfigurationManager.AppSettings["BASE_URL_ONTC"]);
            //    else
            //        Response.Redirect(ConfigurationManager.AppSettings["BASE_URL_NFC"]);

            //    return;
            //}
        }

        BASE_URL_ONTC = ConfigurationManager.AppSettings["BASE_URL_ONTC"];
        BASE_URL_NFC = ConfigurationManager.AppSettings["BASE_URL_NFC"];


        if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IS_DEVELOPMENT"]) && ConfigurationManager.AppSettings["IS_DEVELOPMENT"] == "TRUE")
        {
            IsDevelopment = true;
            buttonsList.Visible = true;

        }
        if (!IsPostBack)
        {
            ddlRole.Items.Add(new ListItem { Text = ERP.Resources.AMM.SelectARole, Value = "" });

            txtUserName.Attributes.Add("placeholder", ERP.Resources.AMM.UserName);
            txtPassword.Attributes.Add("placeholder", ERP.Resources.AMM.Password);

        }
        if (Session.Contents["Culture"] != null && Session.Contents["Culture"].ToString() == "ar-OM")
        {
            pullleft = "pull-right";
            pulllright = "pull-left";
            direction = "rtl";

        }
        if (Session.Contents["Culture"] == null)
        {
            Session.Contents["Culture"] = "en-US";
        }

        // clear the items on every  login
        ERP.LeaveTypeMaster.LeaveTypeMasterItems = null;
        ERPQuickLinks.ClearQuickLinks();
        Teset();
        string al = ERP.Resources.Alert.Immediate;

        var uri2 = Request.Url;
        var host = uri2.GetLeftPart(UriPartial.Authority);

        var url1 = Request.Url.Host +
   (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port.ToString());



        // 21-02-2013
        string ss = ERP.Resources.AMM.ActiveFromDate;
        //DateTime dtt = DateTime.FromOADate(41326);
 


        if (!IsPostBack && RunOnlyInIE)
        {
            CommonTasks.showAlert(@"
$(function() {
    if (!$.browser.msie) {
        $.blockUI({ message: 'Please use Internet Explorer.' });
        window.close();
    }
    else if ((/MSIE ((5\\.5)|6)/.test(navigator.userAgent) && navigator.platform == 'Win32')) {
        $.blockUI({ message: 'Please use Internet Explorer Version7 or Later Version.' });
        window.close();
    }
});
", this);


        }
        Page.Title = ":: Login Form - ERP ::";
        lbl_error.Visible = false;
        //ddlCompany.Attributes.Add("Width", "150px");
        //ddlRole.Attributes.Add("Width", "150px");
        //txtUserName.Attributes.Remove("type");
        txtPassword.Attributes.Remove("type");
        txtPassword.Attributes.Add("type", "password");
        //log_error.write_to_log_file("", "", "SELECT AMM_CHECK_COMPUTER_NAME('" + Server.MachineName.ToUpper() + @"') FROM DUAL");

        //if (objDB.execute_scalar("SELECT AMM_CHECK_COMPUTER_NAME('" + Server.MachineName.ToUpper() + @"') FROM DUAL") != "97130441")
        //{
        //    Session.Clear();
        //    txtPassword.Text = "";
        //     = "";
        //}

        if (ConfigurationManager.AppSettings["SMS_HOME_PAGE_TITLE"] != null)
            lblTitle.Text = ConfigurationManager.AppSettings["SMS_HOME_PAGE_TITLE"];


        if (!IsPostBack)
        {
            if (Request.Cookies["L_USER"] != null)
            {
                //automaticLogin();
                if (CreateSession(Request.Cookies["L_USER"].Value, Request.Cookies["L_COMP_CODE"].Value, Request.Cookies["L_ROLE_ID"].Value, false))
                    Response.Redirect("~/Home.aspx");
            }
            //TA

            if (dbaccess.GetDBObject.execute_scalar(@"SELECT TAP_PARAVALUE FROM TAS_ATT_PARAM  where      TAP_PARANAME ='IS_WINDOWS_AUTH' ") == "Y"
                )
            {

                if ((string.IsNullOrEmpty(Request["AutoLogin"])))
                {


                    string currentUserName = GetCurrentADUserName();

                    string userId = dbaccess.GetDBObject.execute_scalar(@"SELECT USER_NAME FROM PPM_EMPLOYEE_DETAILS, AMM_USER_DETAILS WHERE NVL(PEMP_EMP_ACTIVE,'Y')='Y' and
  PEMP_EMP_CODE =  USER_EMP_CODE and UPPER(PEMP_EMP_NKT_ADDRESS_3) ='" + currentUserName.Trim().ToUpper() + @"' ");
                    string deaultUserRole = dbaccess.GetDBObject.execute_scalar(@"SELECT max(URL_ROLE_ID) FROM AMM_USER_ROLE_LNK_DETAILS, amm_user_details
 WHERE URL_USER_ID= USER_ID and USER_NAME =   '" + userId + @"' ");

                    if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(deaultUserRole))
                    {
                        if (CreateSession(userId, "01", deaultUserRole, false))
                            Response.Redirect("~/Home.aspx");
                    }
                }
            }
        }

        HiddenField2.Value = txtUserName.Value.Trim().ToUpper();

        if (!IsPostBack)
        {
            ViewState["USER_NAME"] = txtUserName.Value;
            ViewState["PASSWORD"] = txtPassword.Value;
        }

        if (IsPostBack)
        {

            Control postbackControlInstance = null;

            for (int i = 0; i < Page.Request.Form.Keys.Count; i++)
            {
                if (Page.Request.Form.Keys[i] == null)
                {
                    continue;
                }
                postbackControlInstance = Page.FindControl(Page.Request.Form.Keys[i]);
                if (postbackControlInstance == null)
                {
                    continue;
                }
                if (postbackControlInstance is System.Web.UI.WebControls.Button)
                {
                    str_post_back_control = Page.Request.Form.Keys[i];
                }

            }
            //ScriptManager.RegisterStartupScript(txtUserName, typeof(TextBox), "alert", "<Script>alert('" + str + "');</Script>", false);

        }
    }

    protected void Button1_Click(object sender, EventArgs e)
        {

        }
    protected void btnLogin_Click(object sender, EventArgs e)
    {
        if (ddlRole.Items.Count == 0)
        {

            ddlRole.DataBind();
            ddlCompany.DataBind();

        }
        if (HiddenField2.Value != "" & txtPassword.Value != "")
        {
            if (ValidateADUser())
            {
                if (CreateSession(HiddenField2.Value.Trim(), ddlCompany.SelectedValue, ddlRole.SelectedValue, chkRemember.Checked))
                {
                    Response.Redirect(@"~/Home.aspx");
                }
                else if (ddlCompany.Items.Count == 0 || ddlRole.Items.Count == 0)
                    //CommonTasks.showAlert(@"alert('Unable to create session.');", this);
                    CommonTasks.showAlert(@"alert('Please choose the Company and Role.');", this);
            }

        }
        else
        {
            lbl_error.Text = "UserName / Password cannot be empty";
            lbl_error.Visible = true;
        }

    }
    //    void get_session_values()
    //    {

    ////        string str_select = @"
    //// ---> Dani Commented this on 30/08/2011 to enable wits_team login for the implementation of our systems. 
    ////-----> The details of the Master Script available with the Wits Manager.
    ////SELECT  USER_ID, USER_SHORT_NAME, USER_EMP_CODE, PEMP_EMP_CODE ,
    ////PEMP_EMP_DEPTARTMENT_CODE, PEMP_EMP_BRANCH_CODE
    ////FROM AMM_USER_DETAILS , PPM_EMPLOYEE_DETAILS  where upper(USER_NAME) = UPPER('" + .Trim().ToUpper().Replace("'", "''") + @"')
    ////AND PEMP_EMP_CODE = USER_EMP_CODE ";

    //        string str_select = @"
    //SELECT  
    //    USER_ID, USER_SHORT_NAME, nvl(USER_EMP_CODE,'wits') USER_EMP_CODE, nvl(PEMP_EMP_CODE,'wits') PEMP_EMP_CODE,
    //    nvl(PEMP_EMP_DEPTARTMENT_CODE,'wits-dep') PEMP_EMP_DEPTARTMENT_CODE, nvl(PEMP_EMP_BRANCH_CODE,'wits_br') PEMP_EMP_BRANCH_CODE,
    //    PBM_BRANCH_CODE, PBM_BRANCH_NAME
    //FROM 
    //    AMM_USER_DETAILS , PPM_EMPLOYEE_DETAILS , PPM_BRANCH_MASTER 
    //where 
    //    upper(USER_NAME) = UPPER('" + .Trim().ToUpper().Replace("'", "''") + @"')
    //    AND PEMP_EMP_CODE (+) = USER_EMP_CODE
    //    AND USER_DEFAULT_LOCATION = PBM_BRANCH_CODE(+)
    //
    //";

    //        string str_select_func_curr_code = "SELECT  ACD_FUNCTIONAL_CURR_CODE,MCUM_DEC_PLACE,MCUM_SUB_UNIT FROM AMM_COMPANY_DETAILS,MMM_CURRENCY_MASTER WHERE UPPER(ACD_COMP_CODE) = '" + ddlCompany.SelectedValue.ToString().ToUpper().Trim().Replace("'", "''") + "' AND UPPER(MCUM_CURR_CODE) = UPPER(ACD_FUNCTIONAL_CURR_CODE) ";

    //        DataTable dt_result = objDB.execute_query_retun_datatable(str_select);
    //        DataTable dt_func_curr = objDB.execute_query_retun_datatable(str_select_func_curr_code);

    //        if (dt_result != null && dt_result.Rows.Count > 0)
    //        {

    //            userDetail.Login_Company_Code = ddlCompany.SelectedValue.ToUpper().Trim();
    //            userDetail.Login_Company_Name = ddlCompany.SelectedItem.Text.Trim();
    //            userDetail.User_Name = .ToUpper().Trim();
    //            userDetail.User_Id = int.Parse(dt_result.Rows[0]["USER_ID"].ToString());
    //            userDetail.User_Display_Name = dt_result.Rows[0]["USER_SHORT_NAME"].ToString();
    //            userDetail.Emp_Code =  dt_result.Rows[0]["USER_EMP_CODE"].ToString() ;
    //            userDetail.Emp_Name = dt_result.Rows[0]["PEMP_EMP_CODE"].ToString();
    //            userDetail.Role_Id = int.Parse(ddlRole.SelectedValue.ToUpper().Trim());
    //            userDetail.DepartmentCode = dt_result.Rows[0]["PEMP_EMP_DEPTARTMENT_CODE"].ToString();

    //            userDetail.BranchCode = dt_result.Rows[0]["PEMP_EMP_BRANCH_CODE"].ToString();

    //            userDetail.DefaultBranchCode = dt_result.Rows[0]["PBM_BRANCH_CODE"].ToString();
    //            userDetail.DefaultBranchDesc = dt_result.Rows[0]["PBM_BRANCH_NAME"].ToString();

    //            userDetail.Role_Logo = objDB.execute_scalar(@"SELECT MCMD_ENTITY_DESC FROM AMM_ROLE_DETAILS, ACCMD_GET_COMMON_MASTER WHERE ROLE_ID = '" + userDetail.Role_Id + @"' 
    //and MCMH_ENTITY_GROUP = 'APP_LOGO'
    //and MCMD_ENTITY_CODE = ROLE_LOGO_PATH");


    //            // L_USER,  L_COMP_CODE ,  L_ROLE_ID
    //            if (chkRemember.Checked)
    //            {
    //                Response.Cookies.Add(new HttpCookie("L_USER") { Expires = DateTime.Now.AddDays(10), Value = .Trim().ToUpper() });
    //                Response.Cookies.Add(new HttpCookie("L_COMP_CODE") { Expires = DateTime.Now.AddDays(10), Value = ddlCompany.SelectedValue.ToUpper() });
    //                Response.Cookies.Add(new HttpCookie("L_ROLE_ID") { Expires = DateTime.Now.AddDays(10), Value = ddlRole.SelectedValue.ToUpper() });
    //            }


    //            Session["userDetail"] = userDetail;

    //        }
    //        if (dt_func_curr != null && dt_func_curr.Rows.Count > 0)
    //        {
    //            userDetail.base_curr_code = dt_func_curr.Rows[0]["ACD_FUNCTIONAL_CURR_CODE"].ToString();
    //            userDetail.base_curr_decimal_places = dt_func_curr.Rows[0]["MCUM_DEC_PLACE"].ToString();
    //            userDetail.CurrencySubUnit = dt_func_curr.Rows[0]["MCUM_SUB_UNIT"].ToString();
    //            Response.Redirect("home.aspx");
    //        }

    //    }



  static  bool CreateSession(string ParUserName, string ParCompCode, string ParRoleId, bool IsRememberMeChecked)
    {
        bool IsSessionCreated = false;
        //        string str_select = @"
        // ---> Dani Commented this on 30/08/2011 to enable wits_team login for the implementation of our systems. 
        //-----> The details of the Master Script available with the Wits Manager.
        //SELECT  USER_ID, USER_SHORT_NAME, USER_EMP_CODE, PEMP_EMP_CODE ,
        //PEMP_EMP_DEPTARTMENT_CODE, PEMP_EMP_BRANCH_CODE
        //FROM AMM_USER_DETAILS , PPM_EMPLOYEE_DETAILS  where upper(USER_NAME) = UPPER('" + .Trim().ToUpper().Replace("'", "''") + @"')
        //AND PEMP_EMP_CODE = USER_EMP_CODE ";
        dbaccess objDB = new dbaccess();

        string str_select = @"
SELECT  PEMP_EMP_NAME,
    USER_ID, USER_SHORT_NAME, nvl(USER_EMP_CODE,'wits') USER_EMP_CODE, nvl(PEMP_EMP_CODE,'wits') PEMP_EMP_CODE,
    nvl(PEMP_EMP_DEPTARTMENT_CODE,'wits-dep') PEMP_EMP_DEPTARTMENT_CODE, PDPM_DEPARTMENT_DESC , 
    PEMP_EMP_DESIGNATION_CODE , PDSM_DESIGNATION_DESC,
    nvl(PEMP_EMP_BRANCH_CODE,'wits_br') PEMP_EMP_BRANCH_CODE,
    PBM_BRANCH_CODE, PBM_BRANCH_NAME,PEMP_EMP_NAME_AR,
    nvl(PEMP_IS_AMINISTRATOR,'N') as PEMP_IS_AMINISTRATOR,
    nvl(PEMP_HR_LV_APPR,'0') as PEMP_HR_LV_APPR, 
    nvl(PEMP_IS_BRANCH_ADMIN,'N') as PEMP_IS_BRANCH_ADMIN , PEMP_REPORTING_TO,nvl(PEMP_BROWSE_EMPLOYEES,'N')PEMP_BROWSE_EMPLOYEES , nvl(PEMP_IS_DEPARTMENT_HEAD,'N') as PEMP_IS_DEPARTMENT_HEAD
FROM 
    AMM_USER_DETAILS , PPM_EMPLOYEE_DETAILS , PPM_BRANCH_MASTER , PPM_DEPARTMENT_MASTER , PPM_DESIGNATION_MASTER
where 
    upper(USER_NAME) = UPPER('" + ParUserName + @"')
    AND PEMP_EMP_CODE (+) = USER_EMP_CODE
    AND USER_DEFAULT_LOCATION = PBM_BRANCH_CODE(+)
    AND PEMP_EMP_DEPTARTMENT_CODE = PDPM_DEPARTMENT_CODE(+)
    AND PEMP_EMP_DESIGNATION_CODE = PDSM_DESIGNATION_CODE
    and  AUD_COMP_CODE = PBM_COMP_CODE (+)
    
";
        //ERP1.LogError.WriteToLogFile("", false, str_select);

        string str_select_func_curr_code = @"
SELECT  
    ACD_FUNCTIONAL_CURR_CODE,MCUM_DEC_PLACE,MCUM_SUB_UNIT 
FROM 
    AMM_COMPANY_DETAILS,MMM_CURRENCY_MASTER 
WHERE UPPER(ACD_COMP_CODE) = '" + ParCompCode + "' AND UPPER(MCUM_CURR_CODE) = UPPER(ACD_FUNCTIONAL_CURR_CODE) ";


        DataTable dt_result = objDB.execute_query_retun_datatable(str_select);
        DataTable dt_func_curr = objDB.execute_query_retun_datatable(str_select_func_curr_code);
        DataTable dtCompany = objDB.execute_query_retun_datatable(@"SELECT ACD_COMP_CODE, ACD_COMP_NAME ,ACD_COUNTRY_CODE , ACM_COUNTRY_NAME FROM AMM_COMPANY_DETAILS, AMM_COUNTRY_MASTER WHERE ACD_COUNTRY_CODE = ACM_COUNTRY_CODE AND ACD_COMP_CODE = '" + ParCompCode + @"'  ");
        UserDatail userDetail = new UserDatail();

        try
        {
            if (dt_result != null && dt_result.Rows.Count > 0 && dtCompany != null && dtCompany.Rows.Count > 0)
            {

                userDetail.IsAdministrator = dt_result.Rows[0]["PEMP_IS_AMINISTRATOR"].ToString().Equals("Y");

                userDetail.CanBrowseAllEmployees = dt_result.Rows[0]["PEMP_BROWSE_EMPLOYEES"].ToString().Equals("Y");
                userDetail.IsHRLeaveApprover = dt_result.Rows[0]["PEMP_HR_LV_APPR"].ToString().Equals("1");
                userDetail.IsBranchAdministrator = dt_result.Rows[0]["PEMP_IS_BRANCH_ADMIN"].ToString().Equals("Y");

                userDetail.IsDepartmentHead = dt_result.Rows[0]["PEMP_IS_DEPARTMENT_HEAD"].ToString().Equals("Y");

                userDetail.Login_Company_Code = dtCompany.Rows[0]["ACD_COMP_CODE"].ToString();
                userDetail.Login_Company_Name = dtCompany.Rows[0]["ACD_COMP_NAME"].ToString();

                userDetail.CompCountryCode = dtCompany.Rows[0]["ACD_COUNTRY_CODE"].ToString();
                userDetail.CompCountryName = dtCompany.Rows[0]["ACM_COUNTRY_NAME"].ToString();

                userDetail.User_Name = ParUserName;
                userDetail.User_Id = dt_result.Rows[0]["USER_ID"].ToString().ToInt();
                userDetail.User_Display_Name = dt_result.Rows[0]["USER_SHORT_NAME"].ToString();
                userDetail.Emp_Name_Arabic = dt_result.Rows[0]["PEMP_EMP_NAME_AR"].ToString();
                userDetail.Emp_Code = dt_result.Rows[0]["USER_EMP_CODE"].ToString();
                userDetail.Emp_Name = dt_result.Rows[0]["PEMP_EMP_NAME"].ToString();
                userDetail.Role_Id = ParRoleId.ToInt();

                userDetail.DepartmentCode = dt_result.Rows[0]["PEMP_EMP_DEPTARTMENT_CODE"].ToString();
                userDetail.DepartmentName = dt_result.Rows[0]["PDPM_DEPARTMENT_DESC"].ToString();
                userDetail.BranchCode = dt_result.Rows[0]["PEMP_EMP_BRANCH_CODE"].ToString();
                userDetail.DefaultBranchCode = dt_result.Rows[0]["PEMP_EMP_BRANCH_CODE"].ToString();
                userDetail.DefaultBranchDesc = dt_result.Rows[0]["PBM_BRANCH_NAME"].ToString();
                userDetail.ManagerEmpCode = dt_result.Rows[0]["PEMP_REPORTING_TO"].ToString();
                userDetail.ManagerEmpName = objDB.execute_scalar("SELECT PEMP_EMP_NAME FROM PPM_EMPLOYEE_DETAILS WHERE PEMP_EMP_CODE = '" + dt_result.Rows[0]["PEMP_REPORTING_TO"].ToString() + "'");
                userDetail.DesignationCode = dt_result.Rows[0]["PEMP_EMP_DESIGNATION_CODE"].ToString(); ;
                userDetail.DesignationName = dt_result.Rows[0]["PDSM_DESIGNATION_DESC"].ToString(); ;
                DataTable dt_Module = objDB.execute_query_retun_datatable(@"SELECT ROLE_ID, MODULE_ID, MODULE_CODE, MODULE_NAME, MODULE_OTH_NAME FROM AMM_ROLE_DETAILS,AMM_MODULES_MASTER 
                WHERE ROLE_MODULE_ID   = MODULE_ID AND ROLE_ID= '" + userDetail.Role_Id + "'     ");//and ROLE_COMP_CODE='" + ParCompCode + @"'
                if (dt_Module != null && dt_Module.Rows.Count > 0)
                {
                    userDetail.ModuleId = int.Parse(dt_Module.Rows[0]["MODULE_ID"].ToString());
                    userDetail.ModuleCode = dt_Module.Rows[0]["MODULE_CODE"].ToString();
                    userDetail.ModuleName = dt_Module.Rows[0]["MODULE_NAME"].ToString();
                    userDetail.ModuleNameAR = dt_Module.Rows[0]["MODULE_OTH_NAME"].ToString();
                }
                userDetail.Role_Logo = objDB.execute_scalar(@"SELECT MCMD_ENTITY_DESC FROM AMM_ROLE_DETAILS, ACCMD_GET_COMMON_MASTER WHERE 
                ROLE_ID = '" + userDetail.Role_Id + @"' 
                and MCMH_ENTITY_GROUP = 'APP_LOGO'
                and MCMD_ENTITY_CODE = ROLE_LOGO_PATH");

                //ROLE_READ_ONLY
                var session = HttpContext.Current.Session;

                session["ROLE_READ_ONLY"] = objDB.execute_scalar(@"SELECT ROLE_READ_ONLY FROM AMM_ROLE_DETAILS  WHERE 
                ROLE_ID = '" + userDetail.Role_Id + @"' ");

                int tasId = 0;
                int.TryParse(objDB.execute_scalar(@"Select F_GET_TAS_ID('01','" + userDetail.Emp_Code + "') from dual"), out tasId);
                userDetail.TASID = tasId;
                // L_USER,  L_COMP_CODE ,  L_ROLE_ID
                //if (IsRememberMeChecked)
                //{
                //    Response.Cookies.Add(new HttpCookie("L_USER") { Expires = DateTime.Now.AddDays(10), Value = ParUserName });
                //    Response.Cookies.Add(new HttpCookie("L_COMP_CODE") { Expires = DateTime.Now.AddDays(10), Value = ParCompCode });
                //    Response.Cookies.Add(new HttpCookie("L_ROLE_ID") { Expires = DateTime.Now.AddDays(10), Value = ParRoleId });
                //}



                if (dt_func_curr != null && dt_func_curr.Rows.Count > 0)
                {
                    userDetail.base_curr_code = dt_func_curr.Rows[0]["ACD_FUNCTIONAL_CURR_CODE"].ToString();
                    userDetail.base_curr_decimal_places = dt_func_curr.Rows[0]["MCUM_DEC_PLACE"].ToString();
                    userDetail.CurrencySubUnit = dt_func_curr.Rows[0]["MCUM_SUB_UNIT"].ToString();
                    //Response.Redirect("home.aspx");
                }
                HttpContext.Current.Session["userDetail"] = userDetail;

                ERP.ERPSiteMapProvider erpSiteMapProvider = new ERP.ERPSiteMapProvider();
                //erpSiteMapProvider.IsInitialized = false;
                //erpSiteMapProvider.ClearSiteMap();
                ERP.ERPSiteMapManager.Clear();
                SessionManager.CreateERPSession(userDetail.User_Name, userDetail.Role_Id, userDetail.Login_Company_Code);
                IsSessionCreated = true;
             

                //AppCode.LogError.WriteToLogFile("111111111111111111111111111111", "", "11111111111111");
                //if (UserSignOnManager.GetInstatnce.IsUserExitsInOtherSession(loggedUser))
                //{
                //    CommonTasks.showAlert("$(function() { ShowDuplicateLoginPage(); } );", this); 
                //    IsSessionCreated = true;
                //}
                //else
                //{
                //    UserSignOnManager.GetInstatnce.AddUser(loggedUser);
                //}
            }
        }
        catch (Exception e1)
        {
            AppCode.LogError.WriteToLogFile("CrateSession(string ParUserName, string ParCompCode, string ParRoleId, bool IsRememberMeChecked)", "", e1.ToString());
        }
        ERP.Log.LogSessionCount.UserLoggedIn();

        return IsSessionCreated;
    }

    void populate_user_company()
    {
        string str_select = "SELECT ";
        str_select += " LUC.ALUC_COMP_CODE, ACD.ACD_COMP_NAME ";
        str_select += " FROM AMM_COMPANY_DETAILS ACD, AMM_LNK_USER_COMP LUC, AMM_USER_DETAILS AUD ";
        str_select += " WHERE LUC.ALUC_COMP_CODE = ACD.ACD_COMP_CODE ";
        str_select += " AND LUC.ALUC_USER_ID = AUD.USER_ID ";
        str_select += " AND UPPER(AUD.USER_NAME) ='" + HiddenField2.Value.ToString().Trim().ToUpper().Replace("'", "''") + "' ";
        // str_select += " and  AUD.USER_PASSWORD = '" + txtPassword.Text.ToString().Trim().Replace("'", "''") + "'";
        //str_select += " AND AUD.USER_PASSWORD ='" + ERP.WitsEncryption.Encrypt(txtPassword.Text.ToString().Trim().Replace("'", "''"), "wits") + "' ";

        //ScriptManager.RegisterStartupScript(txtPassword, typeof(TextBox), "user_password", "<Script>document.getElementById('" + txtPassword.ClientID + "').value='" + txtPassword.Text + "';</Script>", false);
        //SqlDataSource1.SelectCommand = str_select;
        //SqlDataSource1.DataBind();
        ddlCompany.DataBind();


    }

    protected void ddlCompany_SelectedIndexChanged(object sender, EventArgs e)
    {
        populate_user_role();
        ddlRole.DataBind();
    }


    void populate_user_role()
    {
        string str_select = "SELECT ";
        if (ConfigurationManager.AppSettings["IS_ROLE_SORT_ENABLED"] == "TRUE")
            str_select += " ROLE_SORT_ORDER, ";

        str_select += " URL.URL_ROLE_ID, ARD.ROLE_NAME ";
        str_select += " FROM AMM_USER_ROLE_LNK_DETAILS URL, AMM_ROLE_DETAILS ARD, AMM_USER_DETAILS AUD ";
        str_select += " WHERE URL.URL_ROLE_ID = ARD.ROLE_ID ";
        str_select += " AND URL.URL_USER_ID = AUD.USER_ID ";
        str_select += " AND UPPER(AUD.USER_NAME) ='" + HiddenField2.Value.ToString().Trim().ToUpper().Replace("'", "''") + "' ";
        //str_select += " and  AUD.USER_PASSWORD = '" + txtPassword.Text.ToString().Trim().Replace("'", "''") + "'";
        //str_select += " AND AUD.USER_PASSWORD ='" + ERP.WitsEncryption.Encrypt(txtPassword.Text.ToString().Trim().Replace("'", "''"), "wits") + "' ORDER BY  ARD.ROLE_NAME";

        if (ConfigurationManager.AppSettings["IS_ROLE_SORT_ENABLED"] == "TRUE")
            str_select += " ORDER BY ROLE_SORT_ORDER";

        //ERP.ERPCommonSettings.GetERPCommonSettings
        //role_sort_order
        //and URL_COMP_CODE='" + ddlCompany.SelectedValue + @"' \

        //ScriptManager.RegisterStartupScript(txtPassword, typeof(TextBox), "user_password", "<Script>document.getElementById('" + txtPassword.ClientID + "').value='" + txtPassword.Text + "';</Script>", false);
        //SqlDataSource2.SelectCommand = str_select;
        //SqlDataSource2.DataBind();
        ddlRole.DataBind();

        ScriptManager.RegisterStartupScript(ddlRole, typeof(DropDownList), "ddlRole", "document.getElementById('" + ddlRole.ClientID + "').focus();", true);
    }
    bool isDevelopment = true;
    bool ValidateADUser()
    {
        //if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IS_DEVELOPMENT"]))
        //    return true;
        if (dbaccess.GetDBObject.execute_scalar(@"SELECT TAP_PARAVALUE FROM TAS_ATT_PARAM  where      TAP_PARANAME ='IS_AD_AUTHENTICATION' ") == "Y")
        //if (TASSetUpParameters.GetSetupParameters.IsActiveDirectoryEnabled)
        {
            ADAuthentication aDAuthentication = new ADAuthentication();
            if (aDAuthentication.IsUserExistInAD(txtUserName.Value.Trim(), txtPassword.Value))
            // if(true)
            {

                // SELECT PEMP_EMP_CODE FROM PPM_EMPLOYEE_DETAILS WHERE PEMP_EMP_NKT_ADDRESS_3 =''
                string userId = dbaccess.GetDBObject.execute_scalar(@"SELECT USER_NAME FROM PPM_EMPLOYEE_DETAILS, AMM_USER_DETAILS WHERE  NVL(PEMP_EMP_ACTIVE,'Y') ='Y' and
  PEMP_EMP_CODE =  USER_EMP_CODE and UPPER(PEMP_EMP_NKT_ADDRESS_3) ='" + txtUserName.Value.Trim().ToUpper() + @"' ");
                if (!string.IsNullOrEmpty(userId))
                {
                    // = userId.Trim().ToUpper();
                    // = userId.Trim().ToUpper();
                    HiddenField2.Value = userId.Trim().ToUpper();
                    return true;
                }
                CommonTasks.ErrorMessage("The user id is not found in dtabase. Please contact the admin.", this);

                return false;
            }
            else
            {
                CommonTasks.ErrorMessage("The user not found/ password wrong. The active directory authentication failed", this);
                return false;
            }
        }
        else
        {

            string str_chk_user_pass_query = " SELECT COUNT(*) FROM AMM_USER_DETAILS WHERE upper(USER_NAME) = '" + HiddenField2.Value.ToString().Trim().ToUpper().Replace("'", "''") + "' ";
            str_chk_user_pass_query += " AND USER_PASSWORD ='" + txtPassword.Value.ToString().Trim().Replace("'", "''") + "' ";

            if (int.Parse((objDB.record_found(str_chk_user_pass_query).ToString())) <= 0)
            {
                lbl_error.ForeColor = System.Drawing.Color.Red;
                lbl_error.Font.Bold = true;
                lbl_error.Text = "User Name and Password doesnot match.";
                lbl_error.Visible = true;
                return false;
            }
            else
            {
                return true;
                //get_session_values();
                //Response.Redirect("home.aspx");
            }
        }
        //return true;
    }
    //bool validate_username_password()
    //{
    //    if (!ValidateADUser())
    //        return false;

    //    string str_chk_user_pass_query = " SELECT COUNT(*) FROM AMM_USER_DETAILS WHERE upper(USER_NAME) = '" + .ToString().Trim().ToUpper().Replace("'", "''") + "' ";
    //    str_chk_user_pass_query += " AND USER_PASSWORD ='" + txtPassword.Text.ToString().Trim().Replace("'", "''") + "' ";

    //    if (int.Parse((objDB.record_found(str_chk_user_pass_query).ToString())) <= 0)
    //    {
    //        lbl_error.ForeColor = System.Drawing.Color.Red;
    //        lbl_error.Font.Bold = true;
    //        lbl_error.Text = "User Name and Password doesnot match.";
    //        lbl_error.Visible = true;
    //        return false;
    //    }
    //    else
    //    {

    //        return true;
    //        //get_session_values();
    //        //Response.Redirect("home.aspx");
    //    }
    //}

    protected void txtUSerName_TextChanged(object sender, EventArgs e)
    {
        //if (txtUserName.Text != "" && txtPassword.Text != "")
        {
            //populate_user_company();
            //populate_user_role();

        }
    }
    protected void txtPassword_TextChanged(object sender, EventArgs e)
    {
        if (ViewState["USER_NAME"].ToString() == txtUserName.Value && ViewState["PASSWORD"].ToString() == txtPassword.Value)
        {
            //ScriptManager.RegisterStartupScript(ddlRole, typeof(DropDownList), "ddlRole", "document.getElementById('" + ddlRole.ClientID + "').focus();", true);
            CommonTasks.showAlertLater(@"document.getElementById('" + ddlRole.ClientID + "').focus();", this);
            CommonTasks.showAlert(@"document.getElementById('" + ddlRole.ClientID + "').focus();", this);
            ScriptManager.RegisterStartupScript(txtPassword, typeof(TextBox), "user_password111", "<Script>document.getElementById('" + txtPassword.ClientID + "').value ='" + txtPassword.Value + "';</Script>", false);
            return;
        }

        if (txtUserName.Value != "" && txtPassword.Value != "" && str_post_back_control.ToUpper() != "btnLogin".ToUpper())
        {

            if (!ValidateADUser())
            {
                return;
            }

            populate_user_company();
            populate_user_role();
            if (ddlRole.Items.Count >= 1) ddlRole.Focus(); else txtUserName.Focus();
            ScriptManager.RegisterStartupScript(txtPassword, typeof(TextBox), "user_password", "<Script>document.getElementById('" + txtPassword.ClientID + "').value ='" + txtPassword.Value + "';</Script>", false);
            string str_chk_user_pass_query = " SELECT COUNT(*) FROM AMM_USER_DETAILS WHERE upper(USER_NAME) = '" + HiddenField2.Value.ToString().Trim().ToUpper().Replace("'", "''") + "' ";

            //str_chk_user_pass_query += " AND USER_PASSWORD ='" + txtPassword.Text.ToString().Trim().Replace("'", "''") + "' ";
            if (int.Parse((objDB.record_found(str_chk_user_pass_query).ToString())) <= 0)
            {
                ScriptManager.RegisterClientScriptBlock(this, Page.GetType(), "alert_er", "alert('User name and password is not matching.');$('#" + txtUserName.ClientID + "').focus();", true);
            }
            else
            {
                if (ddlRole.Items.Count == 1 && ddlCompany.Items.Count == 1)
                {
                    if (CreateSession(HiddenField2.Value.Trim(), ddlCompany.SelectedValue, ddlRole.SelectedValue, chkRemember.Checked))
                        Response.Redirect(@"~/Home.aspx");
                }
            }
        }

        ViewState["USER_NAME"] = txtUserName.Value;
        ViewState["PASSWORD"] = txtPassword.Value;
    }
 

    protected void LinkButton1_Click(object sender, EventArgs e)
    {
        if (Session.Contents["Culture"] != null)
        {
            if (Session.Contents["Culture"].ToString() == "ar-OM")
            {
                Session.Contents["Culture"] = "en-US";
            }
            else
                Session.Contents["Culture"] = "ar-OM";

        }
        else
        {
            Session.Contents["Culture"] = "en-US";
        }
        Response.Redirect("Login.aspx");
        //if (Session.Contents["UserDetail"] != null)
        //    return RedirectToAction("Index", "Home");
        //else
        //    return RedirectToAction("Index", "Customer");

    }



    protected void CreateSul_Click(object sender, EventArgs e)
    {
        if (CreateSession("SUL", "01", "13", false))
            Response.Redirect(@"~/Home.aspx");
    }
    protected void CreateAdmin_Click(object sender, EventArgs e)
    {

        if (CreateSession("Test", "01", "7", false))
            Response.Redirect(@"~/Home.aspx");
    }
    protected void CreateShinu_Click(object sender, EventArgs e)
    {
        if (CreateSession("OM10007", "01", "13", false))
            Response.Redirect(@"~/Home.aspx");

    }
}
