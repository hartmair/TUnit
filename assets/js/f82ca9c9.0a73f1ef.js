"use strict";(self.webpackChunktunit_docs_site=self.webpackChunktunit_docs_site||[]).push([[6718],{3624:(e,t,s)=>{s.r(t),s.d(t,{assets:()=>u,contentTitle:()=>i,default:()=>d,frontMatter:()=>o,metadata:()=>r,toc:()=>c});const r=JSON.parse('{"id":"tutorial-extras/argument-formatters","title":"Argument Formatters","description":"If you are writing data driven tests, and using custom classes to represent your data, then the test explorer might not show you useful information to distinguish test cases, and instead only show you the class name.","source":"@site/docs/tutorial-extras/argument-formatters.md","sourceDirName":"tutorial-extras","slug":"/tutorial-extras/argument-formatters","permalink":"/TUnit/docs/tutorial-extras/argument-formatters","draft":false,"unlisted":false,"tags":[],"version":"current","sidebarPosition":15,"frontMatter":{"sidebar_position":15},"sidebar":"tutorialSidebar","previous":{"title":"Event Subscribing","permalink":"/TUnit/docs/tutorial-extras/event-subscribing"},"next":{"title":"Tutorial - Assertions","permalink":"/TUnit/docs/category/tutorial---assertions"}}');var n=s(4848),a=s(8453);const o={sidebar_position:15},i="Argument Formatters",u={},c=[];function l(e){const t={code:"code",h1:"h1",header:"header",p:"p",pre:"pre",...(0,a.R)(),...e.components};return(0,n.jsxs)(n.Fragment,{children:[(0,n.jsx)(t.header,{children:(0,n.jsx)(t.h1,{id:"argument-formatters",children:"Argument Formatters"})}),"\n",(0,n.jsx)(t.p,{children:"If you are writing data driven tests, and using custom classes to represent your data, then the test explorer might not show you useful information to distinguish test cases, and instead only show you the class name."}),"\n",(0,n.jsxs)(t.p,{children:["If you want control over how injected arguments appear in the test explorer, you can create a class that inherits from ",(0,n.jsx)(t.code,{children:"ArgumentDisplayFormatter"})," and then decorate your test with the ",(0,n.jsx)(t.code,{children:"[ArgumentDisplayFormatter<T>]"})," attribute."]}),"\n",(0,n.jsx)(t.p,{children:"For example:"}),"\n",(0,n.jsx)(t.pre,{children:(0,n.jsx)(t.code,{className:"language-csharp",children:'    [Test]\n    [MethodDataSource(nameof(SomeMethod))]\n    [ArgumentDisplayFormatter<SomeClassFormatter>]\n    public async Task Test(SomeClass)\n    {\n        await Assert.That(TestContext.Current!.GetTestDisplayName()).IsEqualTo("A super important test!");\n    }\n'})}),"\n",(0,n.jsx)(t.pre,{children:(0,n.jsx)(t.code,{className:"language-csharp",children:'public class MyFormatter : ArgumentDisplayFormatter\n{\n    public override bool CanHandle(object? value)\n    {\n        return value is SomeClass;\n    }\n\n    public override string FormatValue(object? value)\n    {\n        var someClass = (SomeClass)value;\n        return $"One: {someClass.One} | Two: {someClass.Two}";\n    }\n}\n'})})]})}function d(e={}){const{wrapper:t}={...(0,a.R)(),...e.components};return t?(0,n.jsx)(t,{...e,children:(0,n.jsx)(l,{...e})}):l(e)}},8453:(e,t,s)=>{s.d(t,{R:()=>o,x:()=>i});var r=s(6540);const n={},a=r.createContext(n);function o(e){const t=r.useContext(a);return r.useMemo((function(){return"function"==typeof e?e(t):{...t,...e}}),[t,e])}function i(e){let t;return t=e.disableParentContext?"function"==typeof e.components?e.components(n):e.components||n:o(e.components),r.createElement(a.Provider,{value:t},e.children)}}}]);