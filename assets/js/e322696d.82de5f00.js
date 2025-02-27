"use strict";(self.webpackChunktunit_docs_site=self.webpackChunktunit_docs_site||[]).push([[1114],{7212:(e,s,t)=>{t.r(s),t.d(s,{assets:()=>a,contentTitle:()=>l,default:()=>h,frontMatter:()=>i,metadata:()=>n,toc:()=>c});const n=JSON.parse('{"id":"tutorial-extras/setup","title":"Test Set Ups","description":"Most setup for a test can be performed in the constructor (think setting up mocks, assigning fields.)","source":"@site/docs/tutorial-extras/setup.md","sourceDirName":"tutorial-extras","slug":"/tutorial-extras/setup","permalink":"/TUnit/docs/tutorial-extras/setup","draft":false,"unlisted":false,"tags":[],"version":"current","sidebarPosition":1,"frontMatter":{"sidebar_position":1},"sidebar":"tutorialSidebar","previous":{"title":"Tutorial - Extras","permalink":"/TUnit/docs/category/tutorial---extras"},"next":{"title":"Test Clean Ups","permalink":"/TUnit/docs/tutorial-extras/cleanup"}}');var o=t(4848),r=t(8453);const i={sidebar_position:1},l="Test Set Ups",a={},c=[{value:"[Before(HookType)]",id:"beforehooktype",level:2},{value:"[Before(Test)]",id:"beforetest",level:3},{value:"[Before(Class)]",id:"beforeclass",level:3},{value:"[Before(Assembly)]",id:"beforeassembly",level:3},{value:"[Before(TestSession)]",id:"beforetestsession",level:3},{value:"[Before(TestDiscovery)]",id:"beforetestdiscovery",level:3},{value:"[BeforeEvery(HookType)]",id:"beforeeveryhooktype",level:2},{value:"[BeforeEvery(Test)]",id:"beforeeverytest",level:3},{value:"[BeforeEvery(Class)]",id:"beforeeveryclass",level:3},{value:"[BeforeEvery(Assembly)]",id:"beforeeveryassembly",level:3},{value:"[BeforeEvery(TestSession)]",id:"beforeeverytestsession",level:3},{value:"[BeforeEvery(TestDiscovery)]",id:"beforeeverytestdiscovery",level:3},{value:"AsyncLocal",id:"asynclocal",level:2}];function d(e){const s={code:"code",h1:"h1",h2:"h2",h3:"h3",header:"header",p:"p",pre:"pre",...(0,r.R)(),...e.components};return(0,o.jsxs)(o.Fragment,{children:[(0,o.jsx)(s.header,{children:(0,o.jsx)(s.h1,{id:"test-set-ups",children:"Test Set Ups"})}),"\n",(0,o.jsx)(s.p,{children:"Most setup for a test can be performed in the constructor (think setting up mocks, assigning fields.)"}),"\n",(0,o.jsx)(s.p,{children:"However some scenarios require further setup that could be an asynchronous operation.\nE.g. pinging a service to wake it up in preparation for the tests."}),"\n",(0,o.jsxs)(s.p,{children:["For this, we can declare a method with a ",(0,o.jsx)(s.code,{children:"[Before(...)]"})," or a ",(0,o.jsx)(s.code,{children:"[BeforeEvery(...)]"})," attribute."]}),"\n",(0,o.jsx)(s.h2,{id:"beforehooktype",children:"[Before(HookType)]"}),"\n",(0,o.jsx)(s.h3,{id:"beforetest",children:"[Before(Test)]"}),"\n",(0,o.jsx)(s.p,{children:"Must be an instance method. Will be executed before each test in the class it's defined in.\nMethods will be executed bottom-up, so the base class set ups will execute first and then the inheriting classes."}),"\n",(0,o.jsx)(s.h3,{id:"beforeclass",children:"[Before(Class)]"}),"\n",(0,o.jsx)(s.p,{children:"Must be a static method. Will run once before the first test in the class it's defined in starts."}),"\n",(0,o.jsx)(s.h3,{id:"beforeassembly",children:"[Before(Assembly)]"}),"\n",(0,o.jsx)(s.p,{children:"Must be a static method. Will run once before the first test in the assembly it's defined in starts."}),"\n",(0,o.jsx)(s.h3,{id:"beforetestsession",children:"[Before(TestSession)]"}),"\n",(0,o.jsx)(s.p,{children:"Must be a static method. Will run once before the first test in the test session starts."}),"\n",(0,o.jsx)(s.h3,{id:"beforetestdiscovery",children:"[Before(TestDiscovery)]"}),"\n",(0,o.jsx)(s.p,{children:"Must be a static method. Will run once before any tests are discovered."}),"\n",(0,o.jsx)(s.h2,{id:"beforeeveryhooktype",children:"[BeforeEvery(HookType)]"}),"\n",(0,o.jsxs)(s.p,{children:["All [BeforeEvery(...)] methods must be static - And should ideally be placed in their own file that's easy to find, as they can globally affect the test suite, so it should be easy for developers to locate this behaviour.\ne.g. ",(0,o.jsx)(s.code,{children:"GlobalHooks.cs"})," at the root of the test project."]}),"\n",(0,o.jsx)(s.h3,{id:"beforeeverytest",children:"[BeforeEvery(Test)]"}),"\n",(0,o.jsx)(s.p,{children:"Will be executed before every test that will run in the test session."}),"\n",(0,o.jsx)(s.h3,{id:"beforeeveryclass",children:"[BeforeEvery(Class)]"}),"\n",(0,o.jsx)(s.p,{children:"Will be executed before the first test of every class that will run in the test session."}),"\n",(0,o.jsx)(s.h3,{id:"beforeeveryassembly",children:"[BeforeEvery(Assembly)]"}),"\n",(0,o.jsx)(s.p,{children:"Will be executed before the first test of every assembly that will run in the test session."}),"\n",(0,o.jsx)(s.h3,{id:"beforeeverytestsession",children:"[BeforeEvery(TestSession)]"}),"\n",(0,o.jsx)(s.p,{children:"The same as [Before(TestSession)]"}),"\n",(0,o.jsx)(s.h3,{id:"beforeeverytestdiscovery",children:"[BeforeEvery(TestDiscovery)]"}),"\n",(0,o.jsx)(s.p,{children:"The same as [Before(TestDiscovery)]"}),"\n",(0,o.jsx)(s.pre,{children:(0,o.jsx)(s.code,{className:"language-csharp",children:'using TUnit.Core;\n\nnamespace MyTestProject;\n\npublic class MyTestClass\n{\n    private int _value;\n    private static HttpResponseMessage? _pingResponse;\n\n    [Before(Class)]\n    public static async Task Ping()\n    {\n        _pingResponse = await new HttpClient().GetAsync("https://localhost/ping");\n    }\n    \n    [Before(Test)]\n    public async Task Setup()\n    {\n        await Task.CompletedTask;\n        \n        _value = 99;\n    }\n\n    [Test]\n    public async Task MyTest()\n    {\n        await Assert.That(_value).IsEqualTo(99);\n        await Assert.That(_pingResponse?.StatusCode)\n            .IsNotNull()\n            .And.IsEqualTo(HttpStatusCode.OK);\n    }\n}\n'})}),"\n",(0,o.jsx)(s.h2,{id:"asynclocal",children:"AsyncLocal"}),"\n",(0,o.jsxs)(s.p,{children:["If you are wanting to set AsyncLocal values within your ",(0,o.jsx)(s.code,{children:"[Before(...)]"})," hooks, this is supported."]}),"\n",(0,o.jsxs)(s.p,{children:["But to propagate the values into the test framework, you must call ",(0,o.jsx)(s.code,{children:"context.FlowAsyncLocalValues()"})," - Where ",(0,o.jsx)(s.code,{children:"context"})," is the relevant context object injected into your hook method."]}),"\n",(0,o.jsx)(s.p,{children:"E.g."}),"\n",(0,o.jsx)(s.pre,{children:(0,o.jsx)(s.code,{className:"language-csharp",children:'    [BeforeEvery(Class)]\n    public static void BeforeClass(ClassHookContext context)\n    {\n        _myAsyncLocal.Value = "Some Value";\n        context.FlowAsyncLocalValues();\n    }\n'})})]})}function h(e={}){const{wrapper:s}={...(0,r.R)(),...e.components};return s?(0,o.jsx)(s,{...e,children:(0,o.jsx)(d,{...e})}):d(e)}},8453:(e,s,t)=>{t.d(s,{R:()=>i,x:()=>l});var n=t(6540);const o={},r=n.createContext(o);function i(e){const s=n.useContext(r);return n.useMemo((function(){return"function"==typeof e?e(s):{...s,...e}}),[s,e])}function l(e){let s;return s=e.disableParentContext?"function"==typeof e.components?e.components(o):e.components||o:i(e.components),n.createElement(r.Provider,{value:s},e.children)}}}]);