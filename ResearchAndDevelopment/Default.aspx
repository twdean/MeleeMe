<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ResearchAndDevelopment._Default" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <ol class="round">
        <li class="one">
            <h5>Twitter API</h5>
            <asp:Button ID="btnTwitter" runat="server" OnClick="btnTwitter_Click"/>
        </li>
        <li class="two">
            <h5>Facebook</h5>
            <asp:Button ID="btnFacebook" runat="server" OnClick="btnFacebook_Click"/>
        </li>
    </ol>
</asp:Content>
