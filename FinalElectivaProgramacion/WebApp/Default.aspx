<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApp._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main>
        <section class="row" aria-labelledby="aspnetTitle">
            <h1 id="aspnetTitle">Buscar infracciones</h1>
            <p class="lead">Por favor ingrese su patente para consultar las infracciones.</p>
        </section>

        <div class="row">
            <asp:TextBox ID="TextBox" runat="server" />
            <asp:Button ID="Button" Text="Buscar" runat="server" OnClick="Button_onClick" />
        </div>

        <div class="row">
                <div class="container mt-5">
                    <asp:GridView ID="GridView" runat="server" AutoGenerateColumns="false" CssClass="table table-striped table-bordered">
                        <Columns>
                            <asp:BoundField DataField="ID" HeaderText="ID" />
                            <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />
                            <asp:BoundField DataField="Pagada" HeaderText="Pagada" />
                            <asp:TemplateField HeaderText="Descargar Orden Pago">
                                <ItemTemplate>
                                    <asp:Button ID="DownloadButton" runat="server" Text="Descargar" CssClass="btn btn-primary" CommandName="Download" CommandArgument='<%# Eval("ID") %>' OnClick="DownloadButton_Click" Enabled='<%# !(bool)Eval("Pagada") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
        </div>
    </main>

</asp:Content>
