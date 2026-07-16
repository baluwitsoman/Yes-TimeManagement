<%@ Page Title="Job Card Generation" Language="C#" MasterPageFile="~/MasterPageCommon.master"
    AutoEventWireup="true" Inherits="Job_Card_Details" Theme="ERP" CodeFile="Job_Card_Details.aspx.cs" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %> 
<%@ Register Src="../UC/MultiParameterFilter.ascx" TagName="MultiParameterFilter"
    TagPrefix="uc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <style>
        .noPadding
        {
            padding-left: 0px !important;
        }
    </style>
    <asp:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server">
    </asp:ToolkitScriptManager>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div class="page-content">
                <div class="row">
                    <div class="col-md-12">
                        <h3 class="row header smaller lighter blue">
                            <span class="col-sm-11"><i class="icon-certificate"></i>
                                <asp:Label ID="Label1" runat="server" Text="Job Card Entry"></asp:Label></span>                            
                        </h3>
                       
                        <div class="panel panel-default">
                            <div class="panel-body" style="display: block;">
                                <div class="col-sm-10">
                                     <div class="row" runat="server" id="divPartyRow">
                                        <label class="col-sm-3 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="Label4" runat="server" Text="<%$ ERPResources:Finance, Customer %>"></asp:Label>
                                           <asp:Label ID="Label3" runat="server" ForeColor="#019C14" Style="font-family: Verdana;
                                                font-size: 10pt" Text="*"></asp:Label>
                                        </label>
                                        <div class="col-md-6">
                                            <div class="skyinput skyinput-control">
                                                <i class="icon-txt icon-append icon-external-link" onclick="return open_popup_for_party()">
                                                </i>
                                                <asp:TextBox CssClass="form-control" MaxLength="100" ID="MTR_PARTY_CODE" runat="server" ValidationGroup="save" AutoPostBack="true" OnTextChanged="Party_indexchanged" ></asp:TextBox>                                               
                                            </div>  <asp:HiddenField runat="server" ID="MTRPARTYIND" value="AR" />  <asp:HiddenField runat="server" ID="MTRPARTYNAME"  />
                      
                                        </div>
                                       <%-- <div class="col-md-4">
                                            <div class="skyinput skyinput-control">
                                                <i class="icon-txt icon-append icon-ban-circle"></i>
                                                <asp:TextBox CssClass="form-control" MaxLength="100" ID="MTR_PARTY_NAME" runat="server"></asp:TextBox>
                                            </div>
                                        </div>--%>
                                        <div class="col-md-1">
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" ControlToValidate="MTR_PARTY_CODE"
                                                CssClass="danger" ErrorMessage="*"></asp:RequiredFieldValidator>
                                        </div>
                                    </div>
                                     <div class="row">
                                        <label class="col-sm-3 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="Label26" runat="server" Text="Job Location"></asp:Label>
                                            <asp:Label ID="Label27" runat="server" ForeColor="#019C14" Style="font-family: Verdana;
                                                font-size: 10pt" Text="*"></asp:Label>
                                        </label>
                                        <div class="col-md-1">
                                             
                                              <asp:RadioButton ID="rdljobfield" runat="server" Text="Field" Checked="true" GroupName="joblocation"   ClientIDMode="Static" />
                                            
                                        </div>
                                        <div class="col-md-1">
                                                <asp:RadioButton ID="rdljobworkshop" runat="server" Text="Workshop" GroupName="joblocation" ClientIDMode="Static" />
                                        </div>
                                        <div class="col-md-2">
                                              <asp:RadioButton ID="rdlService" runat="server" Text="Service" Checked="false" GroupName="joblocation"   ClientIDMode="Static" />
                                        </div>
										 <div class="col-md-2">
											 <asp:RadioButton ID="rdlParts" runat="server" Text="Parts" Checked="false" GroupName="joblocation"   ClientIDMode="Static" />
										 </div>
                                        <div class="col-md-1">
                                        </div>
                                    </div>

                                 
                                    <div class="row">
                                        <label class="col-sm-3 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="lblAgency" runat="server" Text="<%$ ERPResources:LGTS, Agency %>"></asp:Label>
                                            <asp:Label ID="Label13" runat="server" ForeColor="#019C14" Style="font-family: Verdana; font-size: 10pt"
                                                Text="*"></asp:Label>
                                        </label>
                                        
                                        <div class="col-md-3">                                             
                                                 
                                                <asp:DropDownList ID="ddlbrandlist" runat="server"   CssClass="form-control"></asp:DropDownList>
                                             
                                        </div>
                                        
                                        <div class="col-md-1">                                          
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator11" runat="server" ControlToValidate="ddlbrandlist"
                                                CssClass="danger" ErrorMessage="*"></asp:RequiredFieldValidator>
                                        </div>
                                    </div>
                                    <div  style="height:5px">                                             
                                            
                                        </div>
                               <div class="row">
                                        <label class="col-sm-3 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="Label29" runat="server" Text="Equipment Type"></asp:Label>     
                                             <asp:Label ID="Label11" runat="server" ForeColor="#019C14" Style="font-family: Verdana; font-size: 10pt"
                                                Text="*"></asp:Label>
                                        </label>
                                        <div class="col-md-3"> 
                                                <asp:DropDownList ID="ddlequipment" runat="server"   CssClass="form-control"></asp:DropDownList>
                                       </div>
                                        <div class="col-md-1">
                                             <asp:RequiredFieldValidator InitialValue="0" ID="RequiredFieldValidator1" Display="Dynamic" 
                                      runat="server" ControlToValidate="ddlequipment"
                                            Text="*" ErrorMessage="ErrorMessage"></asp:RequiredFieldValidator>
                                        </div>
                                    </div> 
                                    <div  style="height:5px">                                             
                                            
                                        </div>
                                     <div class="row">
                                        <label class="col-sm-3 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="Label31" runat="server" Text="Type of Service"></asp:Label>
                                            <asp:Label ID="Label9" runat="server" ForeColor="#019C14" Style="font-family: Verdana; font-size: 10pt"
                                                Text="*"></asp:Label> 
                                        </label>
                                        
                                        <div class="col-md-3">  
                                                <asp:DropDownList ID="ddlservicetype" runat="server"   CssClass="form-control"></asp:DropDownList>                                             
                                        </div>
                                        
                                        <div class="col-md-1">
                                          <asp:RequiredFieldValidator InitialValue="0" ID="Req_ID" Display="Dynamic" 
                                     runat="server" ControlToValidate="ddlservicetype"
                                            Text="*" ErrorMessage="ErrorMessage"></asp:RequiredFieldValidator>
                                        <%--    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="ddlservicetype"
                                                CssClass="danger" ErrorMessage="*"></asp:RequiredFieldValidator>--%>
                                        </div>
                                    </div>
                                     <div  style="height:5px">                                             
                                            
                                        </div>
                                     <div class="row">
                                        <label class="col-sm-3 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="Label34" runat="server" Text="Machine/Gen Set Sno"></asp:Label>
                                             
                                        </label>
                                        
                                        <div class="col-md-3">                                             
                                                 
                                                <asp:TextBox ID="txtmachinegensetno" runat="server" MaxLength="25"   CssClass="form-control"></asp:TextBox>
                                             
                                        </div>
                                        <div class="col-md-4">                                             
                                            
                                        </div>
                                        <div class="col-md-1">
                                          
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="ddlequipment"
                                                CssClass="danger" ErrorMessage="*"></asp:RequiredFieldValidator>
                                        </div>
                                    </div>
                                     <div  style="height:5px">                                             
                                            
                                        </div>
                                   <div class="row">
                                        <label class="col-sm-3 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="Label36" runat="server" Text="Job Code"></asp:Label>
                                            <asp:Label ID="Label37" runat="server" ForeColor="#019C14" Style="font-family: Verdana; font-size: 10pt"
                                                Text="*"></asp:Label>
                                        </label>
                                        
                                        <div class="col-md-3">                                             
                                               <div class="skyinput skyinput-control skyinput-padding">                                                
                                                    <i class="icon-txt icon-append icon-external-link" onclick="return open_popup_for_menu_code()">
                                                    </i>   
                                                <asp:TextBox ID="txtjobcode" runat="server"   placeholder="Auto Generated" AutoPostBack="true"   
                                                    CssClass="form-control"   OnTextChanged="jobcode_indexchanged"></asp:TextBox>
                                             </div><asp:HiddenField ID="hidjobcode" runat="server" />
                                        </div>
                                        <div class="col-md-4">                                             
                                            
                                        </div>
                                        <div class="col-md-1">
                                          
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="ddlequipment"
                                                CssClass="danger" ErrorMessage="*"></asp:RequiredFieldValidator>
                                        </div>
                                    </div>
                                     <div  style="height:5px">                                             
                                            
                                        </div>
                                   
                                   

                                    <div class="row" runat="server"  >
                                        <label class="col-sm-3 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="Label25" runat="server" Text="Job Description"></asp:Label>
                                              <asp:Label ID="Label14" runat="server" ForeColor="#019C14" Style="font-family: Verdana; font-size: 10pt"
                                                Text="*"></asp:Label>
                                        </label>
                                        <div class="col-md-6">
                                            <div class="skyinput skyinput-control">
                                                <i class="icon-txt icon-append icon-pencil"></i>
                                                <asp:TextBox MaxLength="200" ID="txtjobdesc" runat="server" CssClass="form-control" TextMode="MultiLine"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="col-md-1">
                                          
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="txtjobdesc"
                                                CssClass="danger" ErrorMessage="*"></asp:RequiredFieldValidator>
                                        </div>
                                    </div>
                                    <div class="row">
                                         
                                       <label class="col-sm-3 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="Label2" runat="server" Text="Job Opening Date & Time"></asp:Label>
                                             <asp:Label ID="Label8" runat="server" ForeColor="#019C14" Style="font-family: Verdana; font-size: 10pt"
                                                Text="*"></asp:Label>
                                        </label>
                                        <div class="col-md-2">
                                            <div class="skyinput skyinput-control">
                                                <i class="icon-txt icon-append icon-calendar"></i>
                                                <asp:TextBox TabIndex="1" MaxLength="10" ID="txtjobopeningdate" CssClass="form-control datepick" onchange="return calculatedate()" autocomplete="off" ValidationGroup="save"
                                                    runat="server"></asp:TextBox>
                                                
                                            </div>
                                        </div>
                                         <div class="col-md-1">
                                          
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="txtjobopeningdate"  
                                                CssClass="danger" ErrorMessage="*"></asp:RequiredFieldValidator>
                                        </div>
                                          
                                            <label class="col-sm-3 control-label no-padding-left" for="form-field-1">
                                                <asp:Label ID="Label23" runat="server" Text="Job Service Date & Time"></asp:Label>
                                               
                                            </label>
                                            <div class="col-md-2">
                                                <div class="skyinput skyinput-control">
                                                    <i class="icon-txt icon-append icon-calendar"></i>
                                                    <asp:TextBox TabIndex="1" MaxLength="10" ID="txtjobservicedate" CssClass="form-control datepick" onchange="return calculatedate()" autocomplete="off"
                                                        runat="server"></asp:TextBox>
                                                    
                                                </div>
                                            </div>
                                       
                                    </div>
                                    <div class="row">
                                         
                                        <label class="col-sm-3 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="Label19" runat="server" Text="Job Closing Date & Time"></asp:Label>
                                        
                                        </label>
                                        <div class="col-md-2">
                                            <div class="skyinput skyinput-control">
                                                <i class="icon-txt icon-append icon-calendar"></i>
                                                <asp:TextBox TabIndex="1" MaxLength="10" ID="txtjobclosingdate" CssClass="form-control datepick" onchange="return calculatedate()" autocomplete="off"
                                                    runat="server"></asp:TextBox>
                                               
                                            </div>
                                        </div>
                                          
                                    </div>
                                    <div class="row ">
                                        <label class="col-sm-3 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="Label15" runat="server" Text="Job Status"></asp:Label>
                                       <asp:Label ID="Label5" runat="server" ForeColor="#019C14" Style="font-family: Verdana;
                                                font-size: 10pt" Text="*"></asp:Label>
                                        </label>
                                        <div class="col-md-3">                                            
                                                <asp:DropDownList ID="ddljobstatus" runat="server"  CssClass="form-control"></asp:DropDownList>
                                        </div>
                                        <div class="col-md-1">                                          
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="ddljobstatus"
                                                CssClass="danger" ErrorMessage="*"></asp:RequiredFieldValidator>
                                        </div>
                                         
                                    </div>
                                     <div class="row ">
                                          <label class="col-sm-3 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="Label16" runat="server" Text=""></asp:Label>
                                      
                                        </label>
                                         </div>
                                   
                                     <div class="row">
                                        <label class="col-sm-3 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="lblChargeHandComts" runat="server" Text="Customer contact Person"></asp:Label>
                                        </label>
                                        <div class="col-md-3">
                                            <div class="skyinput skyinput-control">
                                                <i class="icon-txt icon-append icon-pencil"></i>
                                                <asp:TextBox ID="txtcustomercontact" runat="server" MaxLength="15" CssClass="form-control" ></asp:TextBox>
                                                    
                                            </div>
                                        </div>
                                    </div>
                                    <div class="height:5px"></div>
                                      <div class="row">
                                        <label class="col-sm-3 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="Label7" runat="server" Text="Customer Phone no"></asp:Label>
                                        </label>
                                        <div class="col-md-3">
                                            <div class="skyinput skyinput-control">
                                                <i class="icon-txt icon-append icon-pencil"></i>
                                                <asp:TextBox ID="txtcustphoneno" runat="server" MaxLength="10" CssClass="form-control" ></asp:TextBox>
                                                    
                                            </div>
                                        </div>
                                           <div class="col-md-1">                                          
                                                          <asp:RegularExpressionValidator ID="RegularExpressionValidator1" ControlToValidate="txtcustphoneno" ErrorMessage="Not a number" ValidationExpression="^[0-9]*$" Runat="server"> </asp:RegularExpressionValidator>  
   
                                        </div>
                                    </div>
                                    <div class="height:5px"></div>
                                    <div class="row" runat="server" id="div1">
                                          
                                        <label class="col-sm-3 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="Label17" runat="server" Text="<%$ ERPResources:Finance, SalesMan %>"></asp:Label>
                                         
                                        </label>
                                        <div class="col-md-3">                                            
                                           
                                               <div class="skyinput skyinput-control skyinput-padding">
                                                    <i class="icon-txt icon-append icon-external-link" onclick="return OpenMultiBrowserPopup('EMPLOYEE')">
                                                    </i>
                                                    <asp:TextBox ID="txtEmpId" runat="server" CssClass="form-control" MaxLength="15"  AutoPostBack="true"
                                                        onfocus="this.setAttribute('oldValue',this.value);" onkeyup="this.value=this.value.toUpperCase();"
                                                      OnTextChanged="Emp_indexchanged"  ></asp:TextBox>  <%--onblur="open_popup_for_employee(this)"--%>

                                                </div>
                                                <asp:HiddenField ID="hid_emp_code" runat="server" />
                                            
                                        </div>
                                         
                                       
                                            <label class="col-sm-3 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="Label38" runat="server" Text="<%$ ERPResources:Finance, SalesMan Phone no %>"></asp:Label>
                                            
                                        </label>
                                             <div class="col-md-3">
                                                 
                                               <div class="skyinput skyinput-control">
                                                    <i class="icon-txt icon-append icon-pencil"></i>
                                                    <asp:TextBox CssClass="form-control" MaxLength="10" ID="txtsalesmancontact" runat="server"></asp:TextBox>                                               
                                                 
                                                </div>
                                          </div>
                                          <%-- <div class="col-sm-1">
                                            <asp:RegularExpressionValidator ID="vldNumber" ControlToValidate="txtsalesmancontact"  ErrorMessage="Not a number" ValidationExpression="^[0-9]*$" Runat="server"> </asp:RegularExpressionValidator>  
                                                                </div>--%>
                                       </div>                                        
                                    
                                   
                                    <div class="height:5px"></div>
                                    <div class="row">
                                          
                                        <label class="col-sm-3 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="Label12" runat="server" Text="<%$ ERPResources:Finance, Branch %>"></asp:Label>
                                           
                                        </label>

                                        <div class="col-md-3">
                                          <asp:DropDownList ID="ddlbranch" runat="server"   CssClass="form-control"></asp:DropDownList>
                                         </div>
                                         
                                        <label class="col-sm-3 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="Label10" runat="server" Text="<%$ ERPResources:Finance, Sector %>"></asp:Label>
                                           
                                        </label>
                                        <div class="col-md-3">
                                            <div class="skyinput skyinput-control">
                                                <i class="icon-txt icon-append icon-pencil"></i>
                                                <asp:TextBox CssClass="form-control" MaxLength="100" ID="txtsector" runat="server"></asp:TextBox>
                                               
                                            </div>
                                        </div>
                                         
                                    </div>
                                    <div class="height:5px"></div>
                                    <div class="row">
                                          
                                        <label class="col-sm-3 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="Label6" runat="server" Text="<%$ ERPResources:Finance, Remarks %>"></asp:Label>
                                           
                                        </label>

                                        
                                        <div class="col-md-9">
                                            <div class="skyinput skyinput-control">
                                                <i class="icon-txt icon-append icon-pencil"></i>
                                                <asp:TextBox CssClass="form-control" MaxLength="1000" ID="txtremarks" TextMode="MultiLine"  runat="server"></asp:TextBox>
                                               
                                            </div>
                                        </div>
                                         
                                    </div>
                                    <div class="row">
                                        <label class="col-sm-10 control-label no-padding-right" for="form-field-1">
                                            <asp:Label ID="lblError" ForeColor="Red" runat="server" Text=""></asp:Label>
                                        </label>
                                    </div>
                                    <div class="row">
                                        <div class="col-md-3">
                                        </div>
                                        <div class="col-md-9">
                                            <div class="form-group">
                                                <asp:Button ID="Button1" runat="server" Width="0px" Height="0px" OnClick="Button1_Click" CausesValidation ="false"
                                                    Style="visibility: hidden; background: none !important;" CssClass="hide1" />
                                                <asp:Button ID="btnSave" runat="server" CausesValidation="True" Text="<%$ ERPResources:LGTS, Save %>"
                                                    OnClick="btnSave_Click" ValidationGroup="save" CssClass="btn btn-success btn-sm" OnClientClick="return btnSaveValidation()" />&nbsp;
                                                <asp:Button ID="btnDelete" runat="server" CausesValidation="True" Text="<%$ ERPResources:LGTS, Delete %>"
                                                    CssClass="btn btn-danger btn-sm" ValidationGroup="delete" OnClick="btnDelete_Click"
                                                    OnClientClick="return confirm('Are you sure? Do you want to delete?');" />
                                                 
                                                <asp:Button ID="btnClear" runat="server" CausesValidation="False" Text="<%$ ERPResources:LGTS, Clear %>" OnClick="btnClear_Click"
                                                    class="btn btn-warning btn-sm" />
                                                <asp:Button ID="btnPost" runat="server" CausesValidation="False" Text="<%$ ERPResources:LGTS, Post %>"  visible="false"
                                                    class="btn btn-purple btn-sm" />
                                                <asp:Button ID="btnReport" runat="server" CausesValidation="False"  Text="<%$ ERPResources:LGTS, Report %>"  
                                                    OnClientClick='return gotoreport()' class="btn btn-yellow btn-sm" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div id="div_popup" style="display: none; height: 100%; width: 100%;">
                <div style="float: right; width: 100%; border: 1px solid blue; height: 20px;">
                    <input style="float: right;" type="button" value="Close" onclick="close_pop_up_window()" />
                </div>
                <div id="div_frame_pop_up" style="height: 100%; width: 100%;">
                    <iframe id="frame_pop_up" style="height: 100%; width: 100%;"></iframe>
                    <iframe id="garbage_frame" style="width: 0px; height: 0px; visibility: visible;">
                    </iframe>
                </div>
            </div>
            <script src="<%= ResolveUrl("~/js/jquery-ui-1.9.0.custom.min.js")%>" type="text/javascript"></script>
        </ContentTemplate>
    </asp:UpdatePanel>
    <link href='<%=ResolveUrl("~/css/ui-lightness/jquery-ui-1.9.0.custom.css") %>' rel="stylesheet"
        type="text/css" />
    <script>


        var obj_last_selected_but_id;
    

        var str_company_code = '<%=str_company_code %>';
        
        var divPartyRow = '#<%=divPartyRow.ClientID %>';
        var empname = '#<%= txtEmpId.ClientID %>'
        var empid = $('#<%=hid_emp_code.ClientID %>').val();
        var salescontact = $('#<%=txtsalesmancontact.ClientID%>').val();

        function open_popup_for_menu_code() {
           
            var obj_but_obj = $('#<%=txtEmpId.ClientID%>').val();
            var str_query = "";
           
            str_query = "SELECT   MTJ_COMP_CODE, MTJ_PARTY_IND,MTJ_PARTY_CODE,MPD_BRANCH_CODE, MPD_BRANCH_NAME,MTJ_JOB_LOCATION,MTJ_BRAND,MTJ_EQUIPMENT_TYPE_ID,MTJ_SERVICE_TYPE_ID,MTJ_SERIAL_NO,MTJ_JOB_CODE,MTJ_JOB_DESC,";
            str_query +=" TO_char(MTJ_JOB_OPENING_DATE, 'dd/MM/yyyy') MTJ_JOB_OPENING_DATE, TO_char(MTJ_JOB_SERVICE_START_DATE, 'dd/MM/yyyy') MTJ_JOB_SERVICE_START_DATE, TO_char(MTJ_JOB_CLOSING_DATE, 'dd/MM/yyyy') MTJ_JOB_CLOSING_DATE, MTJ_JOB_STATUS,";
            str_query += " MTJ_CONTACT_PERSON, MTJ_CONTACT_PHONE, MTJ_SALES_EMPLOYEE_CODE, PEMP_EMP_CODE, PEMP_EMP_NAME, MTJ_SALES_EMPLOYEE_PHONE, MTJ_BRANCH, MTJ_SECTOR, MTJ_REMARKS";
            str_query += " FROM MTL_TRANSACTION_JOB_OTHER_DTLS LEFT  JOIN V_EMPLOYEE_DETAILS ON  MTJ_SALES_EMPLOYEE_CODE = PEMP_EMP_CODE , MMM_PARTY_DETAILS   ";
            str_query +=" WHERE   MTJ_PARTY_CODE = MPD_BRANCH_CODE  AND MPD_PARTY_TYPE = 'AR'";
             

            var search_url = '<%=ResolveUrl("~/Logistics/search_return_more_parameters.aspx") %>?cnt=' + Math.random() + "&par_query=" + str_query + "&from=JOBCARD&col_code=MTJ_JOB_CODE&col_desc=MTJ_JOB_DESC";

            OpenMultiBrowserPopupWithUrlWidthHeight(search_url, "500px", "550px");
            return false;

        }
        function open_popup_for_party() {

          
            var part_ind = $('#<%= MTRPARTYIND.ClientID %>').val(); 
            var str_query = "";
            str_query = " SELECT   MPD_BRANCH_CODE, MPD_BRANCH_NAME  FROM  MMM_PARTY_DETAILS WHERE MPD_COMP_CODE='" + str_company_code + "' AND MPD_PARTY_TYPE='" + part_ind + "' ";
            var search_url = '<%=ResolveUrl("~/Logistics/search_return_more_parameters.aspx") %>?cnt=' + Math.random() + "&par_query=" + str_query + "&from=PARTY&col_code=MPD_BRANCH_CODE&col_desc=MPD_BRANCH_NAME";
          
            OpenMultiBrowserPopupWithUrlWidthHeight(search_url, "500px", "550px");
            return false;

        }

        function open_popup_for_employee(obj) {


            var Emp_id = $('#<%= txtEmpId.ClientID %>').val(); alert(Emp_id);
            var str_query = "";
            str_query = " SELECT PEMP_EMP_CODE,PEMP_EMP_NAME,PBM_BRANCH_NAME,PDPM_DEPARTMENT_DESC,PEMP_EMP_DESIGNATION_DESC,PEMP_EMP_PHONE FROM V_EMPLOYEE_DETAILS WHERE PEMP_EMP_ACTIVE='Y' AND PEMP_EMP_CODE='" + Emp_id+"' ";
            var search_url = '<%=ResolveUrl("~/Logistics/search_return_more_parameters.aspx") %>?cnt=' + Math.random() + "&par_query=" + str_query + "&from=EMPLOYEE&col_code=PEMP_EMP_CODE&col_desc=PEMP_EMP_NAME";
            
            OpenMultiBrowserPopupWithUrlWidthHeight(search_url, "500px", "550px");
            return false;
            txtempcode
        }
         
        var g_array_callback_values = new Array();

        function callback_from_return_more_paramers(par_assign_to)// identify the row 
        {


            console.log(g_array_callback_values)

            if (par_assign_to == 'JOBCARD')
            { 

               //    alert(g_array_callback_values["MTJ_JOB_OPENING_DATE"]);
                
                $('#<%= hidjobcode.ClientID %>').val(g_array_callback_values["MTJ_JOB_CODE"]);
               $('#<%= txtjobcode.ClientID %>').val(g_array_callback_values["MTJ_JOB_CODE"]);
                $('#<%= txtjobdesc.ClientID %>').val(g_array_callback_values["MTJ_JOB_DESC"]);
                $('#<%= txtjobopeningdate.ClientID %>').val(g_array_callback_values["MTJ_JOB_OPENING_DATE"]);
                $('#<%= txtjobservicedate.ClientID %>').val(g_array_callback_values["MTJ_JOB_SERVICE_START_DATE"]);
                $('#<%= txtjobclosingdate.ClientID %>').val(g_array_callback_values["MTJ_JOB_CLOSING_DATE"]);
                $('#<%= ddljobstatus.ClientID %>').val(g_array_callback_values["MTJ_JOB_STATUS"]);


                $('#<%= MTRPARTYNAME.ClientID  %>').val(g_array_callback_values["MTJ_PARTY_CODE"]);
                $('#<%= MTRPARTYIND.ClientID  %>').val(g_array_callback_values["MTJ_PARTY_IND"]);
                $('#<%= MTR_PARTY_CODE.ClientID  %>').val(g_array_callback_values["MPD_BRANCH_NAME"]);
					if (g_array_callback_values["MTJ_JOB_LOCATION"] == "Field") {
					$('#rdljobfield').prop('checked', true);
					}
					else if (g_array_callback_values["MTJ_JOB_LOCATION"] == "Service") {
					$('#rdlService').prop('checked', true);
					}
					else if (g_array_callback_values["MTJ_JOB_LOCATION"] == "Parts") {
					$('#rdlParts').prop('checked', true);
					}
					else {
					$('#rdljobworkshop').prop('checked', true);
					}

					$('#<%= ddlbrandlist.ClientID  %>').val(g_array_callback_values["MTJ_BRAND"]);
                $('#<%= ddlequipment.ClientID  %>').val(g_array_callback_values["MTJ_EQUIPMENT_TYPE_ID"]);
                $('#<%= ddlservicetype.ClientID  %>').val(g_array_callback_values["MTJ_SERVICE_TYPE_ID"]);
                $('#<%= txtmachinegensetno.ClientID  %>').val(g_array_callback_values["MTJ_SERIAL_NO"]);
                $('#<%= txtjobopeningdate.ClientID  %>').val(g_array_callback_values["MTJ_JOB_SERVICE_START_DATE"]);
                $('#<%=txtremarks.ClientID  %>').val(g_array_callback_values["MTJ_REMARKS"]);

                $('#<%= txtcustomercontact.ClientID  %>').val(g_array_callback_values["MTJ_CONTACT_PERSON"]);
                $('#<%= txtcustphoneno.ClientID  %>').val(g_array_callback_values["MTJ_CONTACT_PHONE"]);
                $('#<%= hid_emp_code.ClientID  %>').val(g_array_callback_values["MTJ_SALES_EMPLOYEE_CODE"]);
                $('#<%= txtEmpId.ClientID  %>').val(g_array_callback_values["PEMP_EMP_NAME"]);
                $('#<%= txtsalesmancontact.ClientID  %>').val(g_array_callback_values["MTJ_SALES_EMPLOYEE_PHONE"]);
                $('#<%= ddlbranch.ClientID  %>').val(g_array_callback_values["MTJ_BRANCH"]);
                $('#<%= txtsector.ClientID  %>').val(g_array_callback_values["MTJ_SECTOR"]);
                $('#<%= txtremarks.ClientID  %>').val(g_array_callback_values["MTJ_REMARKS"]);
                $('#<%=Button1.ClientID%>').click();

             } 
 

            if (par_assign_to == 'EMPLOYEE') {
                $('#<%= txtEmpId.ClientID %>').val(g_array_callback_values["PEMP_EMP_NAME"]);
                $('#<%= hid_emp_code.ClientID %>').val(g_array_callback_values["PEMP_EMP_CODE"]);
             }

            
            if (par_assign_to == 'PARTY') {
                $('#<%= MTR_PARTY_CODE.ClientID %>').val(g_array_callback_values["MPD_BRANCH_NAME"]);
                $('#<%= MTRPARTYNAME.ClientID %>').val(g_array_callback_values["MPD_BRANCH_CODE"]);
                }
            
        }

        function open_popup(par_table, par_code, par_desc, par_from, par_subquery) {
            var search_url = '<%=ResolveUrl("~/Logistics/search.aspx") %>?qs1=' + Math.random() + "&table_name=" + par_table + "&col_code=" + par_code + "&col_desc=" + par_desc + "&from=" + par_from + "&sub_query=" + par_subquery;
            //window.showModalDialog(search_url, self, "dialogHeight: 400px; dialogWidth: 400px; center: yes;");
            OpenMultiBrowserPopupWithUrlWidthHeight(search_url, "500px", "550px");
            return false;
        }

        function assign_value(par_code, par_desc, par_return_to) {
            return false;
        }

        function PostBack() {
            document.getElementById('<%=Button1.ClientID %>').click();

        }
        function btnSaveValidation() {
            if (!calculatedate()) {
                return false;
            }
                return Page_ClientValidate();
             //return true;
         }

        function gotoreport() {
           <%-- txtDocumentNumber = '<%=MTR_REF_CODE.ClientID %>';--%>
            var jobCode1 = $('#ctl00_ContentPlaceHolder1_txtjobcode').val();

            var IsActiveFlt = '<%= IsActiveFleet%>';
            OpenMultiBrowserPopupWithUrl('./ReportViewer/LgtsReportVIewer.aspx?JOB_CODE=' + jobCode1);
            return false;
            //alert(IsActiveFlt);
            //alert(blnIsActiveFleet);

            //if (IsActiveFlt == "Y") {
            //window.open('frmReports.aspx?SYS_ID=999999&DOC_NO=' + document.getElementById(txtDocumentNumber).value + '&CALLED_FOR=JOB_CARD_CREATION');
            //} else {
                //alert("Fleet is Cancelled");
                //return false;
            //}
        }


        function setup_div_tag(_width) {
            $(document).ready(function () {
                $('#div_popup').css({
                    width: _width,
                    height: "100%",
                    opacity: 1,
                    cursor: null,
                    color: '#fff',
                    backgroundColor: '#000'

                }),

                $('#frame_pop_up').css({
                    width: "100%",
                    height: "100%",
                    opacity: 1,
                    cursor: null,
                    color: '#fff',
                    backgroundColor: '#000'
                }),


                $('#div_frame_pop_up').css({
                    width: _width,
                    height: "430px",
                    opacity: 1,
                    cursor: null,
                    color: '#fff',
                    backgroundColor: '#000'
                })
            });


        }

        function calculatedate() {
            if ($('#<%= txtjobopeningdate.ClientID%>').val() != "" && $('#<%= txtjobclosingdate.ClientID%>').val() != "")
             {
                var fromDate1 = ReturnDate($('#<%= txtjobopeningdate.ClientID%>').val());
                var toDate1 = ReturnDate($('#<%= txtjobclosingdate.ClientID%>').val());
                var servicedate1;
                if (fromDate1 > toDate1) {
                    alert("Opening Date should be greater than Closing Date", "warning");
                    return false; 
                }

                if ($('#<%= txtjobservicedate.ClientID%>').val()!="")
                    servicedate1 = ReturnDate($('#<%= txtjobservicedate.ClientID%>').val());
                if (fromDate1>servicedate1) {
                    alert("Opening Date should be greater than Service  Date", "warning");
                    return false;
                }
                if (toDate1<servicedate1) {
                    alert("Closing Date should be greater than Service  Date", "warning");
                    return false;
                }
            }
           
              return true;
        }

        function ParseInteger(pData) {
            return parseInt(pData);
        }

        function ReturnDate(parStringDate) {
            if (parStringDate == "") {
                return "";
            }
            var splitChar = "/";
            if (parStringDate.indexOf("/") > 1)
                splitChar = "/";
            else if (parStringDate.indexOf("-") > 0)
                splitChar = "-";
            else {
                alert('The date is not in valid format.');
                return new Date();
            }
            var dateArrays = parStringDate.split(splitChar);
           
            return new Date(ParseInteger(dateArrays[2]), ParseInteger(dateArrays[1]) - 1, ParseInteger(dateArrays[0]), 0, 0, 0, 0);

        }
        function close_pop_up_window() {
            $.unblockUI();
            document.getElementById("garbage_frame").src = '<%= ResolveUrl("~/js/Noname.html")%>';


        }
        function close_pop_up_window_by_program() {
            $.unblockUI();
            document.getElementById("garbage_frame").src = '<%= ResolveUrl("~/js/Noname.html")%>';

        }
        function show_pop_up_dialog(_search_url, _from) {
            $('body', $('#frame_pop_up').contents()).html('Loading... Please Wait.');

            _width = 500;
            _left = 0;
            _left = (screen.width - _width) / 2;
            if (_from == 'ADD_ON') {
                _width = 960;
                _left = (screen.width - _width) / 2;
            }
            _left = _left + "px";
            _width = _width + "px";
            setup_div_tag(_width);
            //blockUI blockMsg blockPage
            /* css: {  
            left: "15px",  
            top: "20px",
            height: "450px" },*/
            $(".blockUI.blockMsg.blockPage").css("border", "3px solid red");
            //$("div").hasClass("selected").toString());
            //$( "div").each( function( intIndex ){ alert($(this).hasClass("blockUI blockMsg blockPage")); } );
            //alert($(".blockUI.blockMsg.blockPage"));
            $.blockUI(
    {
        css: {
            left: _left,
            top: "20px",
            height: "450px",
            width: _width
        },

        message: $('#div_popup'),
        overlayCSS: {
            opacity: 0.1
        }
    });

            //blockUI blockMsg blockPage
            //$(".blockUI blockMsg blockPage").css("width","860px");
            //$('.blockUI.blockMsg').center();
            document.getElementById("garbage_frame").src = '<%= ResolveUrl("~/js/Noname.html")%>';
            document.getElementById("frame_pop_up").src = _search_url;
            document.getElementById("garbage_frame").src = '<%= ResolveUrl("~/js/Noname.html")%>';
        }

       <%-- function call_ajax() {
            var str_query = "";
             str_query = " SELECT MTR_REF_CODE, MTR_REF_DESC, TO_CHAR(MTR_REF_DATE,'dd/MM/yyyy') MTR_REF_DATE, MTR_PARTY_IND, MTR_PARTY_CODE,MPD_BRANCH_NAME MTR_PARTY_NAME, MTR_STATUS , PEMP_EMP_CODE, PEMP_EMP_NAME FROM  MTL_TRANSACTION_REFERENCE,MMM_PARTY_DETAILS, AMM_USER_DETAILS, PPM_EMPLOYEE_DETAILS  WHERE MTR_PARTY_CODE=MPD_BRANCH_CODE AND MTR_COMP_CODE='<%=str_company_code %>' ";
            str_query += " AND MTR_REF_CODE='" + $('#<%= MTR_REF_CODE.ClientID %>').val() + "' AND MTR_CREATION_USER_ID = USER_ID     AND  USER_EMP_CODE = PEMP_EMP_CODE";
            if ($('#<%= MTR_REF_CODE.ClientID %>').val() == "") {
                return;
            } 

            $.post('<%=ResolveUrl("~/jquery_action.aspx") %>', { CALLED_FOR: "QUERY_DATABASE_XML_OUTPUT", select_query: str_query },
                  function (jquer_result) {
                      assign_channel_details_from_xml(jquer_result);
                  }
                  );
        }--%>



        //assign values to detail level products 
        function assign_channel_details_from_xml(_xml_data) {           
            //          alert('_xml_data\n\n' + _xml_data);
            var xmlDoc;
            //text = '<?xml version="1.0"?><DocumentElement></DocumentElement>';
            if (window.DOMParser) {
                parser = new DOMParser();
                xmlDoc = parser.parseFromString(_xml_data, "text/xml");
            }
            else // Internet Explorer
            {
                xmlDoc = new ActiveXObject("Microsoft.XMLDOM");
                xmlDoc.async = "false";
                xmlDoc.loadXML(_xml_data);
            }

            if ($(xmlDoc).find('RECORD_FOUND').text() == "NO_RECORD_FOUND") {

                // clear_row_values(obj_last_selected_button);
                return;
            }
  <%--        $('#<%=ChargeHandComts.ClientID  %>').val($(xmlDoc).find('MTR_CHARGEHAND_CMT').text()); --%>


            <%--   $('#<%= MTR_REF_CODE.ClientID %>').val($(xmlDoc).find('MTR_REF_CODE').text());
            $('#<%= MTR_REF_DESC.ClientID %>').val($(xmlDoc).find('MTR_REF_DESC').text());
            $('#<%= MTR_REF_DATE.ClientID %>').val($(xmlDoc).find('MTR_REF_DATE').text());
            $('#<%= MTR_PARTY_IND.ClientID %>').val($(xmlDoc).find('MTR_PARTY_IND').text());--%>
            $('#<%= MTR_PARTY_CODE.ClientID %>').val($(xmlDoc).find('MTR_PARTY_CODE').text());
            $('#<%= MTRPARTYNAME.ClientID %>').val($(xmlDoc).find('MTR_PARTY_NAME').text());
         <%-- $('#<%= MTR_STATUS.ClientID %>').val($(xmlDoc).find('MTR_STATUS').text());

            $(empCode + ',' + empName).val($(xmlDoc).find('PEMP_EMP_CODE').text());--%>  

           // $(txtEmpname).val($(xmlDoc).find('PEMP_EMP_NAME').text()); //PEMP_EMP_CODE, PEMP_EMP_NAME



        }
      <%--  function binddate() {
            $('.ddlparty').change(function () {
              <%--  $('#<%= MTR_PARTY_CODE.ClientID %>').val('');
                $('#<%= MTR_PARTY_NAME.ClientID %>').val(''); 
            });
        }
        binddate();--%>

      

        //var gLastGridRowId = '';
        //function AdditionalTypeParams(_popUpFor) {
        //    if (_popUpFor == 'EMPLOYEE_LEAVE_TYPE') {
        //        return "," + $(gLastGridRowId).find("[id*='EMP_CODE']").val();
        //    }
        //    // BRANCH PBM_BRANCH_CODE
        //}
        //function AdditionalPopParams(_popUpFor) {
        //    //$.growl('dd:' + $(txtFleetIdDesc1).val());
        //    if (_popUpFor == 'FLEET') {
        //        return "&FFI_FLEET_NUMBER=" + $(txtFleetIdDesc1).val();
        //    }

        //}

        function AdditionalACParamsOnSelected(_popUpFor) {
            //$.growl('2:' + $(txtFleetIdDesc1).val());
            if (_popUpFor == 'EMPLOYEE') {
                return "   '" + $(empid).val() + "' ";
            }
            else if (_popUpFor == 'DEPARTMENT') {
               // return "  '" + $(txtDeptCode).val() + "' ";
            }
            else if (_popUpFor == 'BRANCH') {
              //  return "  '" + $(txtBrnachCode).val() + "' ";
            }
        }
        function AssignBrowserPopUpValues(_popUpFor) {
         //alert(g_array_callback_values["PEMP_EMP_BRANCH_CODE"]);
            if (_popUpFor != undefined && _popUpFor == "EMPLOYEE") {
                $('#<%= txtEmpId.ClientID %>').val(g_array_callback_values["PEMP_EMP_NAME"]);
                $('#<%= hid_emp_code.ClientID %>').val(g_array_callback_values["PEMP_EMP_CODE"]);
                $('#<%= txtsalesmancontact.ClientID %>').val(g_array_callback_values["PEMP_EMP_PHONE"]);
                $('#<%= ddlbranch.ClientID%>').val(g_array_callback_values["PEMP_EMP_BRANCH_CODE"]);
            }
        }

        function AssignGridRowFromPopUP(_popUpFor) {
           
            if (_popUpFor == "EMPLOYEE") {
                $(empname).val(g_array_callback_values["PEMP_EMP_NAME"]);
                $(empid).val(g_array_callback_values["PEMP_EMP_CODE"]);
                $(salescontact).val(g_array_callback_values["PEMP_EMP_PHONE"]);
            }
            if (_popUpFor == 'BRANCH') {
               // $(txtBrnachCode).val(g_array_callback_values["PBM_BRANCH_CODE"]);
               // $(txtBranchDesc).val(g_array_callback_values["PBM_BRANCH_NAME"]);
            }

        }
        function AssignGridRowFromAC(_popUpFor, rawJson) {
          

            if (rawJson == 'NODATA') {
                return;
            }
            var jSONObject = eval(rawJson);
            if (_popUpFor == "EMPLOYEE") {
                $(empname).val(jSONObject[0]["PEMP_EMP_NAME"]);
                $(empid).val(jSONObject[0]["PEMP_EMP_CODE"]);
                $(salescontact).val(g_array_callback_values["PEMP_EMP_PHONE"]);
            }
            ClearGridRows(_popUpFor);
            return;
        }
        function ClearGridRows(_popUpFor) {

            if (_popUpFor == 'EMPLOYEE') {
                $(empname).val('');
                $(empid).val('');
            }


        }
     

        function ValidatePopupBeforeOpen(_popUpFor) {
       
            return true;

        }
       
        function adjustWindowHeight() {
           // SetUpFleetRow();
        }
       

    </script>
</asp:Content>
