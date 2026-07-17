<%@ Page Language="C#" AutoEventWireup="true" EnableEventValidation="false" Inherits="LoginAERToLogin"
    meta:resourcekey="PageResource1" UICulture="auto" ClientIDMode="Static" CodeFile="Login.aspx.cs" StylesheetTheme="" Theme="" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" 
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Register Src="~/UC/Includes.ascx" TagName="Includes" TagPrefix="uc1" %>

<!DOCTYPE html>
<!--
Template Name: Metronic - Responsive Admin Dashboard Template build with Twitter Bootstrap 3.3.7
Version: 4.7
Author: KeenThemes
Website: http://www.keenthemes.com/
Contact: support@keenthemes.com
Follow: www.twitter.com/keenthemes
Dribbble: www.dribbble.com/keenthemes
Like: www.facebook.com/keenthemes
Purchase: http://themeforest.net/item/metronic-responsive-admin-dashboard-template/4021469?ref=keenthemes
Renew Support: http://themeforest.net/item/metronic-responsive-admin-dashboard-template/4021469?ref=keenthemes
License: You must have a valid license purchased only from themeforest(the above link) in order to legally use the theme for your project.
-->
<!--[if IE 8]> <html lang="en" class="ie8 no-js"> <![endif]-->
<!--[if IE 9]> <html lang="en" class="ie9 no-js"> <![endif]-->
<!--[if !IE]><!-->
<html lang="en">
<!--<![endif]-->
<!-- BEGIN HEAD -->
<head runat="server">
    <meta charset="utf-8" />
    <title>AER | Authority for Electricity Regulation, Oman</title>
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta content="width=device-width, initial-scale=1" name="viewport" />
    <meta content="" name="author" />
    <!-- BEGIN GLOBAL MANDATORY STYLES -->

    <link href="http://fonts.googleapis.com/css?family=Open+Sans:400,300,600,700&subset=all" rel="stylesheet" type="text/css" />
    <link href="assets/global/plugins/font-awesome/css/font-awesome.min.css" rel="stylesheet" type="text/css" />
    <link href="assets/global/plugins/simple-line-icons/simple-line-icons.min.css" rel="stylesheet" type="text/css" />
    <link href="assets/global/plugins/bootstrap/css/bootstrap.min.css" rel="stylesheet" type="text/css" />
    <link href="assets/global/plugins/bootstrap-switch/css/bootstrap-switch.min.css" rel="stylesheet" type="text/css" />
    <!-- END GLOBAL MANDATORY STYLES -->
    <!-- BEGIN PAGE LEVEL PLUGINS -->
    <link href="assets/global/plugins/select2/css/select2.min.css" rel="stylesheet" type="text/css" />
    <link href="assets/global/plugins/select2/css/select2-bootstrap.min.css" rel="stylesheet" type="text/css" />
    <!-- END PAGE LEVEL PLUGINS -->
    <!-- BEGIN THEME GLOBAL STYLES -->
    <link href="assets/global/css/components-rounded.min.css" rel="stylesheet" id="style_components" type="text/css" />
    <link href="assets/global/css/plugins.min.css" rel="stylesheet" type="text/css" />
    <!-- END THEME GLOBAL STYLES -->
    <!-- BEGIN PAGE LEVEL STYLES -->
    <%--    <link href="assets/pages/css/login-3.min.css" rel="stylesheet" type="text/css">--%>
    <!-- END PAGE LEVEL STYLES -->
    <!-- BEGIN THEME LAYOUT STYLES -->
    <link href="assets/layouts/layout/css/custom4.css" rel="stylesheet" type="text/css" />
    <link href="assets/layouts/layout/css/login-3.css" rel="stylesheet" type="text/css" />
    <link href="assets/global/plugins/bootstrap-sweetalert/sweetalert.css" rel="stylesheet" type="text/css" />

    <!-- END THEME LAYOUT STYLES -->
    <link rel="shortcut icon" href="favicon.ico" />
    <style>
         .centerlogin {
        }

        .box {
            display: flex;
            align-items: center;
            justify-content: center;
            height: 100vh;
        }

        .boxCont {
            width: 750px;
            /*height: 100px;*/
        }

        .centerloginOld {
            box-shadow: 0 5px 8px 0 rgb(0 0 0 / 20%), 0 9px 26px 0 rgb(0 0 0 / 19%);
        }

        html, body, form {
            height: 100vh;
        }
    </style>
</head>
<!-- END HEAD -->
<body style="margin: 0px; background: #FFFFFF;">
    <form runat="server">
        <!-- BEGIN LOGIN  <img src="~/assets/layouts/layout4/img/login.png">-->

        <telerik:RadScriptManager ID="ScriptManager1" runat="server">
        </telerik:RadScriptManager>
        <telerik:RadWindowManager ID="RadWindowManager1" runat="server">
            <Windows>
                <telerik:RadWindow ID="RadWindow1" runat="server">
                </telerik:RadWindow>
            </Windows>
        </telerik:RadWindowManager>
     
             <%--<asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:ConnString1 %>"
                                            ProviderName="<%$ ConnectionStrings:ConnString1.ProviderName %>"></asp:SqlDataSource>--%>
        <div class="box">
            <div class="boxCont">

                
<rsweb:ReportViewer ID="ReportViewer2" runat="server" 
    Width="100%" 
    Height="100%"
    SizeToReportContent="true"
    AsyncRendering="false">
</rsweb:ReportViewer> 

                <div class="centerloginOld" dir='<%= direction%>'>

                    <div class="setTopMargin2">
                        <asp:Label ID="lblTitle" runat="server" Visible="false"></asp:Label>


                        <div class="row ">
                            <div class="col-lg-12 ">
                                <div id="logo_float" class="col-lg-6 divnopad @pullleft" dir='<%= direction%>'>

                                    <img src="assets/layouts/layout/img/login.png" style="width: 100%;  <%= (!IsNFCLogin? "": "height:553px;" )%>;">
                                </div>
                                <div class="col-lg-6 divnopad @pulllright ">

                                    <div runat="server" id="buttonsList">
                                        <asp:Button ID="admin1" OnClick="CreateAdmin_Click" Text=" Admin" runat="server" CssClass="btn showMe" />
                                        <asp:Button ID="admin3" OnClick="CreateSul_Click" Text=" Suaiman" runat="server" CssClass="btn showMe" />
                                        <asp:Button ID="admin2" OnClick="CreateShinu_Click" Text=" Shinu" runat="server" CssClass="btn showMe"  />

                                    </div>
                                    <div class="col-lg-12" style="padding: 2% 3%; text-align: right">
                                        LOGIN
                                    </div>


                                    <div class="col-lg-3 divnopad">
                                    </div>
                                    <div class="col-lg-6 divnopad">
                                        <img src="assets/layouts/layout/img/AER-med.png" style="margin: 0px; height: 115px; width: 100%">
                                    </div>
                                    <div class="col-lg-3 divnopad">
                                    </div>
                                    <div class="col-lg-12" style="margin: 15px; text-align: center; font-size: 16px; font-weight: 900;">
                                        <%--<span class="font-dark bold uppercase">Welcome To  ERP</span>--%>
                                    </div>

                                    <!-- BEGIN LOGIN -->
                                    <div class="col-lg-12 content" runat="server" id="divNormal">
                                        <!-- BEGIN LOGIN FORM -->

                                        <h3 class="form-title"><%=ERP.Resources.AMM.LoginToYourAccount %></h3>
                                        <div class="alert alert-danger display-hide">
                                            <button class="close" data-close="alert"></button>
                                            <span>Enter Password</span>
                                        </div>
                                        <div class="form-group">
                                            <!--ie8, ie9 does not support html5 placeholder, so we just show field title for that-->
                                            <label class="control-label visible-ie8 visible-ie9"><%=ERP.Resources.AMM.UserName %></label>
                                            <div class="input-icon">
                                                <i class="fa fa-user"></i>
                                                <input onchange="Loadroles()" onblur="Loadroles()" id="txtUserName" runat="server" class="form-control placeholder-no-fix txtUserName" type="text" autocomplete="off" name="username" />
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <label class="control-label visible-ie8 visible-ie9"><%=ERP.Resources.AMM.Password %></label>
                                            <div class="input-icon">
                                                <i class="fa fa-lock"></i>
                                                <input onchange="Loadroles()" onblur="Loadroles()" id="txtPassword" runat="server" class="form-control placeholder-no-fix txtPassword" type="password" autocomplete="off" placeholder='<%=ERP.Resources.AMM.Password %>' name="password" />
                                            </div>
                                        </div>

                                        <div class="form-group">
                                            <label class="control-label visible-ie8 visible-ie9"><%=ERP.Resources.AMM.SelectARole %></label>
                                            <div class="input-icon">
                                                <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-control slt-field ddlRole"
                                                    Style="background: #ebebeb;" DataTextField="ROLE_NAME" DataValueField="URL_ROLE_ID"
                                                    meta:resourcekey="ddlRoleResource1">
                                                </asp:DropDownList>
                                               
                                                <asp:HiddenField ID="HiddenField1" runat="server" />
                                                <asp:Label ID="lbl_error" runat="server" meta:resourcekey="lbl_errorResource1"></asp:Label>
                                                <asp:HiddenField ID="HiddenField2" runat="server" />


                                            </div>
                                        </div>

                                        <div class="form-group">
                                              
                                            <div class="col-lg-6" style="padding-left:0px;">
                                                <asp:LinkButton ID="LinkButton1" OnClick="LinkButton1_Click" runat="server">Switch Lang</asp:LinkButton>
                                                </div>

                                             <div class="col-lg-6" style="padding-right:0px;text-align:right;">
                                                 <label class="rememberme mt-checkbox mt-checkbox-outline">
                                                <input type="checkbox" name="chkRemember" id="chkRemember" value="1" runat="server" />
                                                <%=ERP.Resources.AMM.RememberMe %>
                                                <span></span>
                                            </label>
                                                 </div>
                                            </div>                                           
                                        
                                            
                                           
                                      

                                          
                                        <div class="form-group">
                                               
                                            <div class="col-lg-6" style="padding-left:0px;">
                                            <a href="#" onclick="forgetpwd(this);" style="display:none" >Forgot Password</a>
                                                </div>
                                              <div class="col-lg-1" >
                                            <input type="button" id="btnLogin" class="btn green pull-right btnLogin" value='<%=ERP.Resources.AMM.Login %>' />
                                                    </div>
                                            <div class="col-lg-6" >
                                            <input type="button"   class="btn green pull-right btnLoginByOTP" value='<%=ERP.Resources.AMM.Login %>' />

                                           
                                                
                                                </div>
                                        </div>
                                          


                                        <asp:DropDownList ID="ddlCompany" runat="server" CssClass="slt-field"
                                            Visible="false" AutoPostBack="true">
                                        </asp:DropDownList>



                                        <div class="form-group danger">
                                            <p class="font-red "></p>
                                        </div>

                                        </div>
                                        <!-- END LOGIN FORM -->

                                    </div>

                                     <div class="col-lg-6 content"  id="divForgot" style="display:none;">
                                          <h3 class="form-title">Forgot Password</h3>
                                         <div class="form-group">
                                            <label class="control-label "><%=ERP.Resources.AMM.UserName %></label>
                                            <div class="input-icon">
                                                <i class="fa fa-user"></i>
                                                <input  id="txtforgotusername" runat="server" class="form-control placeholder-no-fix txtforgotusername" maxlength="50" type="text" autocomplete="off" name="txtforgotusername" />
                                            </div>
                                         </div>
                                          <div class="form-group">
                                           <label class="control-label">Please enter your email address to verify for your account</label>
                                              <div class="input-icon">
                                                <i class="fa fa-mail-forward"></i>
                                                <input  id="txtusermail" class="form-control placeholder-no-fix txtusermail" type="text" autocomplete="off"
                                                    placeholder='Enter your mail' name="txtusermail" maxlength="50" />
                                                 
                                                  
                                            </div> 
                                         </div>
                                          <div class="form-group"><a href="#" onclick="fungotologin()" >Goto Login</a>
                                                   <input type="button" id="btnSubmit" class="btn green pull-right" value='Submit' />
                                              </div>
                                    </div>

                                    <div class="col-lg-6 content" runat="server" visible="false" id="divONTC">
                                        <!-- BEGIN LOGIN FORM -->

                                        <h3 class="form-title"><%=ERP.Resources.AMM.LoginToYourAccount %></h3>
                                        <div class="alert alert-danger display-hide">
                                            <button class="close" data-close="alert"></button>
                                            <span>Enter Password</span>
                                        </div>
                                        <div class="form-group">
                                            <!--ie8, ie9 does not support html5 placeholder, so we just show field title for that-->
                                            <label class="control-label visible-ie8 visible-ie9"><%=ERP.Resources.AMM.UserName %></label>
                                            <div class="input-icon">
                                                <i class="fa fa-user"></i>
                                                <input onchange="LoadrolesONTC()" id="ontcUserName" class="form-control placeholder-no-fix ontcUserName" type="text" autocomplete="off" name="ontcUserName" />
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <label class="control-label visible-ie8 visible-ie9"><%=ERP.Resources.AMM.Password %></label>
                                            <div class="input-icon">
                                                <i class="fa fa-lock"></i>
                                                <input onchange="LoadrolesONTC()" id="ontcPassword" class="form-control placeholder-no-fix ontcPassword" type="password" autocomplete="off"
                                                    placeholder='' name="ontcPassword" />
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <label class="control-label visible-ie8 visible-ie9">Choose Company</label>
                                            <div class="input-icon">
                                                <asp:DropDownList onchange="LoadrolesONTC()" ID="ontcCompanyDDL" runat="server" CssClass="form-control slt-field ontcCompanyDDL"
                                                    Style="background: #ebebeb;">
                                                    <asp:ListItem Value="" Text="Choose">Choose Company</asp:ListItem>
                                                    <asp:ListItem Value="ONTC" Text="ONTC">ONTC</asp:ListItem>
                                                    <asp:ListItem Value="NFC" Text="NFC">NFC</asp:ListItem>
                                                </asp:DropDownList>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <label class="control-label visible-ie8 visible-ie9"><%=ERP.Resources.AMM.SelectARole %></label>
                                            <div class="input-icon">
                                                <asp:DropDownList ID="ontcROlesDDL" runat="server" CssClass="form-control slt-field ontcROlesDDL"
                                                    Style="background: #ebebeb;" DataTextField="ROLE_NAME" DataValueField="URL_ROLE_ID"
                                                    meta:resourcekey="ddlRoleResource1">
                                                </asp:DropDownList>
                                                
                                                <asp:HiddenField ID="HiddenField3" runat="server" />
                                                <asp:Label ID="Label1" runat="server" meta:resourcekey="lbl_errorResource1"></asp:Label>
                                                <asp:HiddenField ID="HiddenField4" runat="server" />


                                            </div>
                                        </div>

                                        <div class="create-account">
                                            <p>
                                                <asp:LinkButton ID="LinkButton2" OnClick="LinkButton1_Click" runat="server">Switch Lang</asp:LinkButton>

                                            </p>
                                        </div>

                                        <div class="form-actions">
                                            <label class="rememberme mt-checkbox mt-checkbox-outline">
                                                <input type="checkbox" name="chkRemember" id="Checkbox1" value="1" runat="server" />
                                                <%=ERP.Resources.AMM.RememberMe %>
                                                <span></span>
                                            </label>
                                            <input type="button" id="btnLoginONTC" class="btn green pull-right btnLoginONTC" value='<%=ERP.Resources.AMM.Login %>' />
                                            <input type="button"   class="btn green pull-right btnLoginByOTP" value='<%=ERP.Resources.AMM.Login %>' />
                                        </div>


                                        <asp:DropDownList ID="DropDownList2" runat="server" CssClass="slt-field"
                                            Visible="false" AutoPostBack="true">
                                        </asp:DropDownList>



                                        <div class="form-group danger">
                                            <p class="font-red "></p>
                                        </div>


                                        <!-- END LOGIN FORM -->

                                    </div>
                                </div>
                            </div>
                        </div>

                    </div>
                </div>
            </div>
 

        <!-- OTP Dialog (initially hidden) -->
<div id="otpDialog" title="Enter OTP" style="display:none;">
    <p>Please enter the OTP sent to your email.</p>
    <input type="text" id="txtOtp" maxlength="6" class="form-control" />
    <div id="otpMessage" style="color:red;margin-top:5px;"></div>
</div>


        
        <script src="js/ag-grid-enterprise.min.js"></script>


        <script src='<%= ResolveUrl("~/js/MultiBrowser.js") %>'></script>

        <script src="assets/global/plugins/respond.min.js"></script>
        <script src="assets/global/plugins/excanvas.min.js"></script>
        <script src="assets/global/plugins/ie8.fix.min.js"></script>

        <!-- BEGIN CORE PLUGINS -->
        <script src="assets/global/plugins/jquery.min.js" type="text/javascript"></script>
        <script src="assets/global/plugins/bootstrap/js/bootstrap.min.js" type="text/javascript"></script>
        <script src="assets/global/plugins/js.cookie.min.js" type="text/javascript"></script>
        <script src="assets/global/plugins/jquery-slimscroll/jquery.slimscroll.min.js" type="text/javascript"></script>
        <script src="assets/global/plugins/jquery.blockui.min.js" type="text/javascript"></script>
        <script src="assets/global/plugins/bootstrap-switch/js/bootstrap-switch.min.js" type="text/javascript"></script>
        <!-- END CORE PLUGINS -->
        <!-- BEGIN PAGE LEVEL PLUGINS -->
        <script src="assets/global/plugins/jquery-validation/js/jquery.validate.min.js" type="text/javascript"></script>
        <script src="assets/global/plugins/jquery-validation/js/additional-methods.min.js" type="text/javascript"></script>
        <link rel="stylesheet" href="https://code.jquery.com/ui/1.13.2/themes/base/jquery-ui.css">

        <script src="https://code.jquery.com/ui/1.13.2/jquery-ui.min.js"></script>

        <script src="assets/global/plugins/select2/js/select2.full.min.js" type="text/javascript"></script>
        <!-- END PAGE LEVEL PLUGINS -->
        <!-- BEGIN THEME GLOBAL SCRIPTS -->
        <script src="assets/global/scripts/app.min.js" type="text/javascript"></script>
        <!-- END THEME GLOBAL SCRIPTS -->
        <!-- BEGIN PAGE LEVEL SCRIPTS -->
        <script src="assets/pages/scripts/login.js" type="text/javascript"></script>


        <script src="assets/global/plugins/bootstrap-sweetalert/sweetalert.min.js" type="text/javascript"></script>
        <script src="assets/global/plugins/jquery.blockui.min.js" type="text/javascript"></script>
        <script src='<%= ResolveUrl("~/assets/global/plugins/bootstrap-growl/jquery.bootstrap-growl.min.js") %>'></script>


<%--        <script src="assets/global/plugins/bootstrap-growl/jquery.bootstrap-growl.min.js"></script>--%>
        <!-- END PAGE LEVEL SCRIPTS -->
        <!-- BEGIN THEME LAYOUT SCRIPTS -->
        <!-- END THEME LAYOUT SCRIPTS -->
        <%--<input type="file" id="FileInput1"/>
        <input type="button" id="bbbbb" onclick="uploadFileBase64()"/>--%>
        
        <script>
            var nfcLogin = '<%=System.Configuration.ConfigurationManager.AppSettings["IS_NFC_LOGIN"]%>';
            function SendOTP() {

                var userName = '';
                if (nfcLogin == 'TRUE') {
                    userName = $("#ontcUserName").val();
                }
                else {
                    userName = $("#txtUserName").val();

                }
              //  alert(userName);
                $.ajax({
                    type: "POST",
                    url: "Login.aspx/GenerateOTP",   // WebMethod URL
                    data: JSON.stringify({ userName: userName }),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (response) {
                        App.unblockUI();

                        var res = response.d; // for WebMethod (res = { Success: true/false, FailureReason: ... })

                        if (res.Success) {
                            $("#otpMessage").css("color", "green").text("OTP is sent!");
                            setTimeout(function () {
                                showOtpDialog();
                                //$("#otpDialog").dialog("close");
                            }, 1000);
                        } else {
                            alert("OTP failed: " + res.FailureReason);
                        }
                    },
                    error: function () {
                        App.unblockUI();

                        $("#otpMessage").text("Error validating OTP. Please try again.");
                    }
                });

            }
            //ddlRole, txtPassword, txtUserName
            $(function () {
                // Create dialog
                $("#otpDialog").dialog({
                    autoOpen: false,
                    modal: true,
                    buttons: {
                        "Validate OTP": function () {
                            var otp = $("#txtOtp").val();
                            var roleId = $("#ddlRole").val();
                            var userName1 = '';
                            if (nfcLogin == 'TRUE') {
                                roleId = $("#ontcROlesDDL").val();
                                userName1 = $("#ontcUserName").val()
                            }
                            else {
                                roleId = $("#ddlRole").val();
                                userName1 = $("#txtUserName").val();
                            }

                           
                            if (!otp) {
                                $("#otpMessage").text("Please enter the OTP.");
                                return;
                            }
                            if (!roleId) {
                                alert('Enter role id ');
                                return;
                            }

                            $.ajax({
                                type: "POST",
                                url: "Login.aspx/ValidateOtp",   // WebMethod URL
                                data: JSON.stringify({ userName: userName1, enteredOtp: otp, RoleId: roleId }),
                                contentType: "application/json; charset=utf-8",
                                dataType: "json",
                                success: function (response) {
                                    var res = response.d; // for WebMethod (res = { Success: true/false, FailureReason: ... })

                                    if (res.Success) {
                                        $("#otpMessage").css("color", "green").text("OTP verified successfully!");
                                        setTimeout(function () {
                                            $("#otpDialog").dialog("close");
                                            window.location.href = "Home.aspx"; // redirect after login
                                        }, 1000);
                                    } else {
                                        $("#otpMessage").css("color", "red").text("OTP failed: " + res.FailureReason);
                                    }
                                },
                                error: function () {
                                    $("#otpMessage").text("Error validating OTP. Please try again.");
                                }
                            });
                        },
                        Cancel: function () {
                            $(this).dialog("close");
                        }
                    }
                });
            });

            // Open OTP dialog after password login is OK
            function showOtpDialog() {
                $("#otpMessage").text("");
                $("#txtOtp").val("");
                $("#otpDialog").dialog("open");
            }

            var IsOTPLoginEnabledYN = '<%=IsOTPLoginEnabledYN.ToString().ToLower()%>';
            $(function () {
                //if otplogin enabled
                if (IsOTPLoginEnabledYN == 'true') {
                    $('#btnLogin,#btnLoginByOTP').hide();
                    $('.btnLoginByOTP').show();
                }
                else {
                    $('.btnLoginByOTP').hide();
                }

                $('.txtUserName').focus();
                $('#btnLogin').click(function () {
                   
                    
                    loginClicked();
                });

                $('#btnSubmit').click(function () {
                    sendforgetpwd();
                });
                $('.btnLoginByOTP').click(function () {
                    //showOtpDialog();
                    App.blockUI({ boxed: true });

                    SendOTP();
                });
            });


            function fungotologin() {
                window.location = 'Login.aspx';
            }

            function loginClicked() {
                try {

                } catch (e) {

                }
                
                if ($('.txtUserName').val() == '' || $('.txtPassword').val() == '') {
                    alert('Enter user name and password');
                    return;
                }
                App.blockUI({ boxed: true });
             
                var url1 = 'Ess-aER/ESSHomeData.aspx?Method=AER_LOGIN&UserName=' + $('.txtUserName').val()
                $.ajax({
                    url: url1,
                    type: "POST",
                    data: new FormData($('form')[0]),
                    cache: false,
                    contentType: false,
                    processData: false,
                    success: function (data1) {
                        var data;

                        try {


                            if (data1.data == "SUCCESS") {
                                //chkRemember
                                if ($('input[name*="chkRemember"]').prop("checked")) {
                                    createCookie("L_COMP_CODE", "01", 10);
                                    createCookie("L_USER", $('.txtUserName').val(), 10);
                                    createCookie("L_ROLE_ID", $('.ddlRole').val(), 10);
                                }
                                window.location = 'Home.aspx';
                            }
                            else if (data1.apexStatus) {
                                let json1 = data1;// JSON.parse(data1);
                                if (json1.apexStatus == "TRUE")
                                    window.location = json1.urlToSend;
                                else
                                    alert(json1.errorMsg);
                            }
                            else if (data1.data == "DUPLICATE") {
                                //chkRemember
                                OpenMultiBrowserPopupWithUrlWidthHeight('<%= ResolveUrl("~/Amm/DuplicateLogin.aspx") %>', '600px', '500px');

                            }
                            else {
                                App.unblockUI();
                                if (data1 == '') {
                                    window.location = 'Login.aspx';
                                    return;
                                }
                                alert(data1.data);
                            }



                        } catch (e) {
                            App.unblockUI();
                            return;
                        }

                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        //debugger;
                        App.unblockUI(); alert(textStatus, errorThrown, jqXHR);
                        $('#myApproval').html('No data.');
                    }
                });


            }
            function createCookie(name, value, days) {
                var expires;

                if (days) {
                    var date = new Date();
                    date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
                    expires = "; expires=" + date.toGMTString();
                } else {
                    expires = "";
                }
                document.cookie = encodeURIComponent(name) + "=" + encodeURIComponent(value) + expires + "; path=/";
            }

            function readCookie(name) {
                var nameEQ = encodeURIComponent(name) + "=";
                var ca = document.cookie.split(';');
                for (var i = 0; i < ca.length; i++) {
                    var c = ca[i];
                    while (c.charAt(0) === ' ')
                        c = c.substring(1, c.length);
                    if (c.indexOf(nameEQ) === 0)
                        return decodeURIComponent(c.substring(nameEQ.length, c.length));
                }
                return null;
            }

            function LoadOTPRoles() {
                App.blockUI({ boxed: true });

                $.ajax({
                    type: "POST",
                    url: "Login.aspx/GEtRoles",   // WebMethod URL
                    data: JSON.stringify({ userName: $("#txtUserName").val()  }),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (response) {
                        App.unblockUI();
                        var res = response.d; // for WebMethod (res = { Success: true/false, FailureReason: ... })
                        $('.ddlRole').empty();
                        $('.ddlRole').append($('<option>', {
                            value: "",
                            text: "<%= ERP.Resources.AMM.SelectARole %>"
                        }));

                        if (res.IsUserFound) {
                            data = res.Roles;// JSON.parse(data1);
                            $.each(data, function (key, item) {
                                 $('.ddlRole').append($('<option>', {
                                    value: item.URL_ROLE_ID,
                                    text: item.ROLE_NAME
                                }));
                            });

                        } else {
                         //   $("#otpMessage").css("color", "red").text("OTP failed: " + res.FailureReason);
                            alert('User/password not found.');
                        }
                    },
                    error: function () {
                        App.unblockUI();
                        //$("#otpMessage").text("Error validating OTP. Please try again.");
                    }
                });
            }
            function Loadroles() {

                try {

                } catch (e) {

                }
              
                if ($('.txtUserName').val() == '' || $('.txtPassword').val() == '') {
                    return;
                }
                if (IsOTPLoginEnabledYN == 'true') {
                    LoadOTPRoles();
                    return;
                }
                App.blockUI({ boxed: true });
 
                var url1 = 'Ess-aER/ESSHomeData.aspx?Method=USER_ROLES&UserName=' + $('.txtUserName').val()
                //

                $.ajax({
                    url: url1,
                    type: "POST",
                    data: new FormData($('form')[0]),
                    cache: false,
                    contentType: false,
                    processData: false,
                    success: function (data1) {
                        App.unblockUI();
                        var data;
                        $('.ddlRole').empty();
                        $('.ddlRole').append($('<option>', {
                            value: "",
                            text: "<%= ERP.Resources.AMM.SelectARole %>"
                        }));
                        try {
                            data = data1;// JSON.parse(data1);
                            // alert(data.length);
                             App.unblockUI();
                            //URL_ROLE_ID, ARD.ROLE_NAME
                            $.each(data, function (key, item) {
                                //alert(item.ROLE_NAME);
                                $('.ddlRole').append($('<option>', {
                                    value: item.URL_ROLE_ID,
                                    text: item.ROLE_NAME
                                }));
                            });

                            if (data.length == 1) {
                                $('.ddlRole option:eq(1)').prop('selected', true);  // To select via value
                                //$('#btnLogin').click();
                            }

                        } catch (e) {
                           // alert('User name or password not found.');;
                            return;
                        }

                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                                                    App.unblockUI();

                        alert(textStatus, errorThrown, jqXHR);
                        $('#myApproval').html('No data.');
                    }
                });


            }

            function sendforgetpwd() {
               

                if ($('.txtforgotusername').val() == '' ) {
                    alert('Enter your user name');
                    return;
                }
                if ($('.txtusermail').val() == '') {
                    alert('Enter your mail id');
                    return;
                }
                App.blockUI({ boxed: true });

                var url1 = 'Ess-aER/ESSHomeData.aspx?Method=Forgotpassword&UserName=' + $('.txtforgotusername').val() + '&UserEmail=' + $('.txtusermail').val();
                $.ajax({
                    url: url1,
                    type: "POST",
                    data: new FormData($('form')[0]),
                    cache: false,
                    contentType: false,
                    processData: false,
                    success: function (data1) {
                        var data;
                        console.log('oooo',data1)
                        console.log(data1)
                         //alert(data1.IsFailed);
                        try {


                            if (data1.IsFailed) {
                                swal(data1.Message);
                                App.unblockUI();

                                return;
                            }
                            else if (!data1.IsFailed) {

                                swal(data1.Message);
                                App.unblockUI();

                                
                                return;
                            }
                            


                        } catch (e) {
                            App.unblockUI();
                            return;
                        }

                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        //debugger;
                        App.unblockUI(); alert(textStatus, errorThrown, jqXHR);
                        
                    }
                });


            }



        </script>

        <script>
            //ddlRole, txtPassword, txtUserName

            $(function () {
                $('.ontcUserName').focus();
                $('.btnLoginByOTP,.btnLoginONTC').hide();
                if (IsOTPLoginEnabledYN == 'true') {
                    $('.btnLoginONTC').hide();
                    $('.btnLoginByOTP').show();
                }
                else {
                    $('.btnLoginONTC').show();
                    $('.btnLoginByOTP').hide();
                }
                $('#btnLoginONTC').click(function () {
                   
                    loginClicked2();
                });
            });

            var BASE_URL_ONTC = '<%= BASE_URL_ONTC%>';
            var BASE_URL_NFC = '<%=BASE_URL_NFC %>';
            var URLFirstPart = '<%= URLFirstPart%>';
            function loginClicked2() {
                try {

                } catch (e) {

                }

                //ontcUserName ontcPassword ontcCompanyDDL ontcROlesDDL
                if ($('.ontcUserName').val() == '' || $('.ontcPassword').val() == '' || $('.ontcROlesDDL').val() == '') {
                    alert('Enter user name and password');
                    return;
                }
                //if ($('.txtUserName').val() == '' || $('.txtPassword').val() == '') {
                //    alert('Enter user name and password');
                //    return;
                //}
                App.blockUI({ boxed: true });


                var base1 = "";
                var url1 = 'Ess-aER/ESSHomeData.aspx?Method=AER_LOGIN2&UserName=';

                // assign base url based on users company selection
                if ($('.ontcCompanyDDL').val() == "ONTC")
                    base1 = URLFirstPart + '/' + BASE_URL_ONTC;
                else //if ($('.ontcCompanyDDL').val() == "ONTC")
                    base1 = URLFirstPart + '/' + BASE_URL_NFC;

                var baseURL2 = base1 + '/' + url1;

                $.ajax({
                    url: baseURL2,
                    type: "POST",
                    data: new FormData($('form')[0]),
                    cache: false,
                    contentType: false,
                    processData: false,
                    success: function (data1) {
                        var data;

                        try {

                            console.log(data1)
                           // alert(data1.data)
                            if (data1.data == "SUCCESS") {
                                //chkRemember
                                if ($('input[name*="chkRemember"]').prop("checked")) {
                                    createCookie("L_COMP_CODE", "01", 10);
                                    createCookie("L_USER", $('.txtUserName').val(), 10);
                                    createCookie("L_ROLE_ID", $('.ddlRole').val(), 10);
                                }
                                //alert(base1 + '/Home.aspx');
                                window.location = base1 + '/Home.aspx';
                            }
                            else if (data1.data == "DUPLICATE") {
                                //chkRemember
                                OpenMultiBrowserPopupWithUrlWidthHeight('<%= ResolveUrl("~/Amm/DuplicateLogin.aspx") %>', '600px', '500px');
                            }
                            else {
                                App.unblockUI();
                                if (data1.data == '') {
                                    window.location = 'Login.aspx';
                                    return;
                                }
                                alert(data1.data);
                            }



                        } catch (e) {
                            App.unblockUI();
                            return;
                        }

                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        //debugger;
                        App.unblockUI(); alert(textStatus, errorThrown, jqXHR);
                        // $('#myApproval').html('No data.');
                    }
                });


            }



            function LoadONTCRolesByOTP() {
                App.blockUI({ boxed: true });

                $.ajax({
                    type: "POST",
                    url: "Login.aspx/GEtRoles",   // WebMethod URL
                    data: JSON.stringify({ userName: $("#ontcUserName").val() }),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (response) {
                        App.unblockUI();
                        var res = response.d; // for WebMethod (res = { Success: true/false, FailureReason: ... })
                        $('.ontcROlesDDL').empty();
                        $('.ontcROlesDDL').append($('<option>', {
                            value: "",
                            text: "<%= ERP.Resources.AMM.SelectARole %>"
                        }));
                        //OTPRequired
                        //btn green pull-right btnLoginByOTP btnLoginONTC
                        if (res.IsUserFound) {
                            data = res.Roles;// JSON.parse(data1);
                            $.each(data, function (key, item) {
                                $('.ontcROlesDDL').append($('<option>', {
                                    value: item.URL_ROLE_ID,
                                    text: item.ROLE_NAME
                                }));
                            });
                            if (res.OTPRequired) {
                                $('.btnLoginByOTP').show();
                                $('.btnLoginONTC').hide();
                            }
                            else {
                                $('.btnLoginByOTP').hide();
                                $('.btnLoginONTC').show();
                            }
                        } else {
                            //   $("#otpMessage").css("color", "red").text("OTP failed: " + res.FailureReason);
                            alert('User/password not found.');
                        }
                    },
                    error: function () {
                        App.unblockUI();
                        //$("#otpMessage").text("Error validating OTP. Please try again.");
                    }
                });
            }

            function LoadrolesONTC() {
                try {

                } catch (e) {

                }
                //ontcUserName ontcPassword ontcCompanyDDL ontcROlesDDL
                if ($('.ontcUserName').val() == '' || $('.ontcPassword').val() == '' || $('.ontcCompanyDDL').val() == '') {
                    return;
                }

                if (IsOTPLoginEnabledYN == 'true') {
                    $('.btnLoginByOTP,.btnLoginONTC').hide();
                    LoadONTCRolesByOTP();
                    return;
                }

                App.blockUI({ boxed: true });
                //var url1 = 'ESSHomeData.aspx/GetMyApproval';

                var url1 = 'Ess-aER/ESSHomeData.aspx?Method=USER_ROLES_ONTC&UserName=';
                //

                $.ajax({
                    url: url1,
                    type: "POST",
                    data: new FormData($('form')[0]),
                    cache: false,
                    contentType: false,
                    processData: false,
                    success: function (data1) {
                        App.unblockUI();
                        var data;
                        $('.ontcROlesDDL').empty();
                        $('.ontcROlesDDL').append($('<option>', {
                            value: "",
                            text: "<%= ERP.Resources.AMM.SelectARole %>"
                        }));
                        if (data1.IsFailed) {
                            alert(data1.Message);
                            return;
                        }
                        try {
                            data = data1;//JSON.parse(data1);
                            console.log(data1);
                            // alert(data.length);
                            // App.unblockUI();
                            //URL_ROLE_ID, ARD.ROLE_NAME
                            $.each(data, function (key, item) {
                                //alert(item.ROLE_NAME);
                                $('.ontcROlesDDL').append($('<option>', {
                                    value: item.URL_ROLE_ID,
                                    text: item.ROLE_NAME
                                }));
                            });

                            if (data.length == 1) {
                                $('.ontcROlesDDL option:eq(1)').prop('selected', true);  // To select via value
                            }
                        } catch (e) {
                            alert('User name or password not found.');;
                            return;
                        }

                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        //debugger;
                        alert(textStatus, errorThrown, jqXHR);
                        //$('#myApproval').html('No data.');
                    }
                });


            }

            function forgetpwd() {
                $('#divNormal').hide();
                $('#divForgot').show();
            }

            function convertFileToBase64(file) {
                return new Promise((resolve, reject) => {
                    const reader = new FileReader();
                    reader.readAsDataURL(file);
                    reader.onload = () => resolve(reader.result);
                    reader.onerror = (error) => reject(error);
                });
            }

            async function uploadFileBase64() { // Marked the function as 'async'
                const fileInput1 = document.getElementById('FileInput1');

                // Check if there is a file selected
                if (fileInput1.files.length === 0) {
                    console.error("No file selected.");
                    return;
                }

                console.log("base64String", JSON.stringify(fileInput1.files[0]));
                try {
                      base64String = await convertFileToBase64(fileInput1.files[0]);
                    console.log("base64String", JSON.stringify(base64String));

                    // Remove the 'data:image/jpeg;base64,' prefix from the base64 string
                    base64String = base64String.replace('data:text/plain;base64,', '');
                    base64String = base64String.replace('data:image/png;base64,', '');
                    base64String = base64String.replace('data:image/jpg;base64,', '');
                    base64String = base64String.replace('data:image/jpeg;base64,', '');
                    base64String = base64String.replace('data:image/png;base64,', '');
                    //data:image/jpeg;base64,
                    //const pureBase64String = base64String.replace('data:text/plain;base64,', '');

                    // Send the base64 string in the request
                    const response = await fetch('http://185.226.125.107/EntryRequest/api/Home/Base64', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ 'base64File': base64String })
                    });

                    const data = await response.json();
                    console.log(data);
                } catch (error) {
                    console.error(error);
                }
            }


        </script>

        

    </form>
</body>
</html>
