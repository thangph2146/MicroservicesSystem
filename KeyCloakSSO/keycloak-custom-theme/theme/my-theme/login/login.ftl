<#import "template.ftl" as layout>

<@layout.registrationLayout displayInfo=true; section>
    <#if section = "form">
        <div id="kc-form">
            <form id="kc-form-login" action="${url.loginAction}" method="post">
                <input type="text" id="username" name="username" placeholder="Tên đăng nhập" required />
                <input type="password" id="password" name="password" placeholder="Mật khẩu" required />
                <button type="submit">Đăng nhập</button>
            </form>
            <div>
                <a href="${url.registrationUrl}"><button type="button">Đăng ký</button></a>
            </div>
        </div>
    </#if>
</@layout.registrationLayout> 