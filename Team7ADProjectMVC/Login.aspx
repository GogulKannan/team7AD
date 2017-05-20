<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Team7ADProjectMVC.Login" %>

<%--Authors: Zhan Seng, Gogul/Linda (design)--%>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" href="~/Resources/mystyle.css">
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css">
</head>
<body>
    <form id="form1" runat="server">
        <div class="row">

            <div id="pic" class=" hidden-xs col-sm-5 col-md-8 t7-container">
                <asp:Image ID="Image1" runat="server" src="Resources/bbb.jpg" Style="width: 100%; height: 1080px;" />
            </div>


            <div class="col-xs-12 col-sm-7 col-md-4 t7-container" align="center">
                <br />
                <br />
                <br />
                <br />
                <img src="Resources/finallogo.jpg" style="height:250px;width:300px" />
                <h1 style="font-size: 60px" class="w3-text-shadow t7-white t7-text-indigo t7-center">&nbsp;Logic University
                    <br />
                    SSIS</h1>
                <br />
                <br />
                
              
                
                <asp:Login ID="Login1" runat="server" DestinationPageUrl="Auth" CssClass="t7-white" Height="300px" Width="180px">

                    <LayoutTemplate>
                        <asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="UserName"  CssClass=" t7-left" Width="273px">Sign in with your user ID</asp:Label>
                        <center style="height: 222px; width: 391px">
                            <br />
                            <br />
                       <div class="input-group">
                            <span class="input-group-addon"><i class="glyphicon glyphicon-user"></i></span>
                            <asp:TextBox ID="UserName" runat="server" Class="form-control" placeholder="Enter User ID"></asp:TextBox>
                       </div> 
                             <div  class="t7-text-red">     
                       <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName" ErrorMessage="User Id is required." ToolTip="User Id is required." ValidationGroup="Login1"></asp:RequiredFieldValidator>
                            </div>    
                       <%-- <asp:Label ID="PasswordLabel" runat="server" AssociatedControlID="Password" CssClass="t7-large">Password:</asp:Label>--%>

                       <div class="input-group">
                            <span class="input-group-addon"><i class="glyphicon glyphicon-lock"></i></span>
                            <asp:TextBox ID="Password" runat="server" Class="form-control" TextMode="Password" placeholder="Enter password"></asp:TextBox>
                       </div>
                             <div  class="t7-text-red">   
                       <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password" ErrorMessage="Password is required." ToolTip="Password is required." ValidationGroup="Login1"></asp:RequiredFieldValidator>
                                 </div>
                        <div  class="t7-text-red">
                            <asp:Literal ID="FailureText" runat="server"  EnableViewState="False" ></asp:Literal>
                        </div>
                       
                       <br />
                       <asp:Button ID="LoginButton" runat="server" CommandName="Login" Text="Log In" CssClass="t7-btn t7-left t7-blue" ValidationGroup="Login1" OnClick="LoginButton_Click" />
                    </LayoutTemplate>
                </asp:Login>
            </div>
        </div>
 </form>

</body>
</html>
