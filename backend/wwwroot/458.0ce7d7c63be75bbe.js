"use strict";(self.webpackChunkcrosswords=self.webpackChunkcrosswords||[]).push([[458],{3458:(q,_,l)=>{l.r(_),l.d(_,{UserCrosswordsModule:()=>k});var m=l(6895),v=l(655),w=l(9933),x=l(1135),M=l(3900),b=l(9646),P=l(9300),O=l(7404),t=l(4650),S=l(5384),A=l(3910),a=l(433),F=l(4457),z=l(8e3),y=l(5635),p=l(7724),T=l(6616),Z=l(7044),U=l(1664),$=l(826);function J(n,o){if(1&n&&t._UZ(0,"nz-option",8),2&n){const e=o.$implicit;t.Q6J("nzLabel",e.name)("nzValue",e.id)}}function I(n,o){if(1&n&&t._UZ(0,"nz-option",8),2&n){const e=o.$implicit;t.Q6J("nzLabel",e.name)("nzValue",e.id)}}function L(n,o){if(1&n&&(t.TgZ(0,"nz-select",9),t.YNc(1,I,1,2,"nz-option",2),t.qZA()),2&n){const e=t.oxw();t.xp6(1),t.Q6J("ngForOf",e.crosswordTypeOptions)}}function N(n,o){if(1&n&&t._UZ(0,"nz-option",8),2&n){const e=o.$implicit;t.Q6J("nzLabel",e.name)("nzValue",e.id)}}function Q(n,o){if(1&n){const e=t.EpF();t.TgZ(0,"button",13),t.NdJ("click",function(){t.CHM(e);const i=t.oxw(2);return t.KtG(i.onTakePrompt())}),t._uU(1),t.qZA()}if(2&n){const e=o.ngIf,r=t.oxw(2);t.Q6J("disabled",r.isSelectedCellSolved||!r.isCellSelected||e.promptCount<=0),t.xp6(1),t.hij(" \u0412\u0437\u044f\u0442\u044c \u043f\u043e\u0434\u0441\u043a\u0430\u0437\u043a\u0443 (",e.promptCount,") ")}}function E(n,o){if(1&n&&(t.TgZ(0,"section",10)(1,"h2"),t._uU(2,"\u041a\u0440\u043e\u0441\u0441\u0432\u043e\u0440\u0434\u044b"),t.qZA(),t.TgZ(3,"nz-select",11),t.YNc(4,N,1,2,"nz-option",2),t.ALo(5,"async"),t.qZA(),t.YNc(6,Q,2,2,"button",12),t.ALo(7,"async"),t.qZA()),2&n){const e=t.oxw();t.Q6J("formGroup",e.crosswordsForm),t.xp6(4),t.Q6J("ngForOf",t.lcZ(5,3,e.crosswords$)),t.xp6(2),t.Q6J("ngIf",t.lcZ(7,5,e.selectedCrossword$))}}function D(n,o){if(1&n){const e=t.EpF();t.TgZ(0,"input",20),t.NdJ("ngModelChange",function(i){t.CHM(e);const s=t.oxw().index,c=t.oxw().index,d=t.oxw(2);return t.KtG(d.onLetterChange(i,c,s))}),t.qZA()}if(2&n){const e=t.oxw(),r=e.index,i=e.$implicit,s=t.oxw().index,c=t.oxw(2);t.Q6J("disabled",c.solvedMatrix[s][r])("ngModel",i)}}function B(n,o){if(1&n){const e=t.EpF();t.TgZ(0,"td",18),t.NdJ("click",function(){const i=t.CHM(e),s=i.index,c=i.$implicit,d=t.oxw().index,u=t.oxw(2);return t.KtG(u.onLetterClick(d,s,c))}),t.YNc(1,D,1,2,"input",19),t.qZA()}if(2&n){const e=o.$implicit,r=o.index,i=t.oxw().index,s=t.oxw(2);t.ekj("potentian-letter",void 0!==e)("selected",i===(null==s.selectedCell?null:s.selectedCell.y)&&r===(null==s.selectedCell?null:s.selectedCell.x)),t.xp6(1),t.Q6J("ngIf",void 0!==e)}}function Y(n,o){if(1&n&&(t.TgZ(0,"tr"),t.YNc(1,B,2,5,"td",17),t.qZA()),2&n){const e=o.$implicit;t.xp6(1),t.Q6J("ngForOf",e)}}function W(n,o){if(1&n&&(t.TgZ(0,"nz-table",14,15),t.ALo(2,"crosswordWidth"),t.TgZ(3,"tbody"),t.YNc(4,Y,2,1,"tr",16),t.qZA()()),2&n){const e=o.ngIf,r=t.MAs(1);t.Udp("width",t.lcZ(2,6,e),"px"),t.Q6J("nzData",e)("nzShowPagination",!1)("nzFrontPagination",!1),t.xp6(4),t.Q6J("ngForOf",r.data)}}function V(n,o){if(1&n&&(t.TgZ(0,"tr")(1,"td"),t._uU(2),t.qZA()()),2&n){const e=o.$implicit,r=t.oxw(2);t.xp6(1),t.ekj("selected-definition",-1!==r.selectedWordsIds.indexOf(e.id)),t.xp6(1),t.hij(" ",e.definition," ")}}const G=function(){return{y:"300px"}};function H(n,o){if(1&n&&(t.TgZ(0,"nz-table",21,22)(2,"thead")(3,"tr")(4,"th"),t._uU(5,"\u041e\u043f\u0440\u0435\u0434\u0435\u043b\u0435\u043d\u0438\u0435"),t.qZA()()(),t.TgZ(6,"tbody"),t.YNc(7,V,3,3,"tr",16),t.qZA()()),2&n){const e=o.ngIf,r=t.MAs(1);t.Q6J("nzData",e.words)("nzScroll",t.DdM(5,G))("nzShowPagination",!1)("nzFrontPagination",!1),t.xp6(7),t.Q6J("ngForOf",r.data)}}var g=(()=>{return(n=g||(g={})).UNSTARTED="unstarted",n.STARTED="started",g;var n})();let f=class{constructor(o,e,r,i){this.api=o,this.modal=e,this.formBuilder=r,this.notify=i,this.themes$=new x.X([]),this.filterForm=this.formBuilder.group({theme:[0],crosswordType:[g.STARTED]}),this.crosswordTypeOptions=[{id:g.STARTED,name:"\u041d\u0430\u0447\u0430\u0442\u044b\u0435"},{id:g.UNSTARTED,name:"\u041d\u0435\u043d\u0430\u0447\u0430\u0442\u044b\u0435"}],this.crosswords$=new x.X([]),this.crosswordsForm=this.formBuilder.group({crossword:[0]}),this.selectedCrossword$=new x.X(null),this.crosswordMatrix$=new x.X([[]]),this.solvedMatrix=[],this.selectedCell=null,this.selectedWordsIds=[],this.filterForm.valueChanges.pipe((0,w.t)(this)).subscribe(s=>{!s.crosswordType||!s.theme||(this.updateCrosswordsList(s.theme,s.crosswordType),this.deselectCrossword())}),this.crosswordsForm.get("crossword")?.valueChanges.pipe((0,w.t)(this),(0,M.w)(s=>s?this.selectedCrosswordType===g.STARTED?this.api.getStartedCrossword(s):this.api.getUnstartedCrossword(s):(0,b.of)(null)),(0,P.h)(s=>!!s)).subscribe(s=>this.selectedCrossword$.next(s)),this.selectedCrossword$.pipe((0,w.t)(this)).subscribe(s=>{!s||this.updateCrossword(s.size.height,s.size.width,s.words,s.grid)}),this.updateThemes()}get selectedThemeId(){return this.filterForm.get("theme")?.value}get selectedCrosswordId(){return this.crosswordsForm.get("crossword")?.value}get selectedCrosswordType(){return this.filterForm.get("crosswordType")?.value}get filtersPresent(){return!!this.filterForm.get("theme")?.value&&!!this.filterForm.get("crosswordType")?.value}get isCellSelected(){return Number.isInteger(this.selectedCell?.x)&&Number.isInteger(this.selectedCell?.y)}get isSelectedCellSolved(){if(!this.selectedCell)return!1;const o=this.selectedCell?.x,e=this.selectedCell?.y;return this.solvedMatrix[e][o]}ngOnInit(){}updateCrossword(o,e,r,i=[]){const s=[],c=[];for(let d=0;d<o;d++)s[d]=new Array(e),c[d]=new Array(e);for(const d of r){if(d.p1.x!==d.p2.x)for(let u=0;u<=d.p2.x-d.p1.x;u++){const h=d.p1.y,C=d.p1.x+u;s[h][C]="",d.isSolved&&(c[h][C]=!0)}if(d.p1.y!==d.p2.y)for(let u=0;u<=d.p2.y-d.p1.y;u++){const h=d.p1.y+u,C=d.p1.x;s[h][C]="",d.isSolved&&(c[h][C]=!0)}}if(i.length)for(const d of i)s[d.y][d.x]=" "===d.l?"":d.l;this.solvedMatrix=c,this.crosswordMatrix$.next(s)}onTakePrompt(){const o=this.selectedCell?.x,e=this.selectedCell?.y;this.api.takePrompt(this.selectedCrosswordId,o,e).subscribe(r=>{this.selectedCrossword$.value&&this.selectedCrossword$.value.promptCount--;const i=[],s=this.crosswordMatrix$.value;for(let c=0;c<s.length;c++)i[c]=[...s[c]];i[e][o]=r.letter,this.crosswordMatrix$.next(i),r.solvedWords?.length&&this.applySolvedWords(r.solvedWords)},r=>{this.notify.error("\u041e\u0448\u0438\u0431\u043a\u0430",r.error.message)})}onLetterClick(o,e,r){if(void 0===r)return;this.selectedCell={x:e,y:o};const i=this.selectedCrossword$.value;!i||(this.selectedWordsIds=(0,O.z)(i?.words,e,o).map(s=>s.id))}onLetterChange(o,e,r){this.api.changeLetter(this.selectedCrosswordId,r,e,o||" ").subscribe(s=>{!s.solvedWords?.length||this.applySolvedWords(s.solvedWords)},s=>{this.notify.error("\u041e\u0448\u0438\u0431\u043a\u0430",s.error.message)})}applySolvedWords(o){for(const e of o){const r=this.selectedCrossword$.value?.words.find(i=>i.id===e.id);if(r){if(r.p1.x!==r.p2.x)for(let i=0;i<=r.p2.x-r.p1.x;i++)this.solvedMatrix[r.p1.y][r.p1.x+i]=!0;if(r.p1.y!==r.p2.y)for(let i=0;i<=r.p2.y-r.p1.y;i++)this.solvedMatrix[r.p1.y+i][r.p1.x]=!0}}}updateThemes(){this.api.getThemes().subscribe(o=>this.themes$.next(o))}updateCrosswordsList(o,e){(e===g.STARTED?this.api.getStartedCrosswordList(o):this.api.getUnstartedCrosswordList(o)).subscribe(i=>this.crosswords$.next(i))}deselectCrossword(){this.selectedCell=null,this.crosswordsForm.get("crossword")?.setValue(null,{emitEvent:!1}),this.selectedCrossword$.next(null),this.crosswordMatrix$.next([[]])}};f.\u0275fac=function(o){return new(o||f)(t.Y36(S.s),t.Y36(A.Sf),t.Y36(a.qu),t.Y36(F.zb))},f.\u0275cmp=t.Xpm({type:f,selectors:[["app-user-crosswords"]],decls:11,vars:12,consts:[[1,"themes",3,"formGroup"],["nzShowSearch","","nzPlaceHolder","\u0412\u044b\u0431\u0435\u0440\u0438\u0442\u0435 \u0442\u0435\u043c\u0443","formControlName","theme"],[3,"nzLabel","nzValue",4,"ngFor","ngForOf"],["nzShowSearch","","nzPlaceHolder","\u0412\u044b\u0431\u0435\u0440\u0438\u0442\u0435 \u0442\u0438\u043f \u043a\u0440\u043e\u0441\u0441\u0432\u043e\u0440\u0434\u0430","formControlName","crosswordType",4,"ngIf"],["class","crossword-header",3,"formGroup",4,"ngIf"],[1,"crossword-content"],["class","crossword-table","nzBordered","","nzTableLayout","fixed","nzSize","small",3,"width","nzData","nzShowPagination","nzFrontPagination",4,"ngIf"],["class","definition-table",3,"nzData","nzScroll","nzShowPagination","nzFrontPagination",4,"ngIf"],[3,"nzLabel","nzValue"],["nzShowSearch","","nzPlaceHolder","\u0412\u044b\u0431\u0435\u0440\u0438\u0442\u0435 \u0442\u0438\u043f \u043a\u0440\u043e\u0441\u0441\u0432\u043e\u0440\u0434\u0430","formControlName","crosswordType"],[1,"crossword-header",3,"formGroup"],["nzShowSearch","","nzPlaceHolder","\u0412\u044b\u0431\u0435\u0440\u0438\u0442\u0435 \u043a\u0440\u043e\u0441\u0441\u0432\u043e\u0440\u0434","formControlName","crossword"],["nz-button","","nzType","primary",3,"disabled","click",4,"ngIf"],["nz-button","","nzType","primary",3,"disabled","click"],["nzBordered","","nzTableLayout","fixed","nzSize","small",1,"crossword-table",3,"nzData","nzShowPagination","nzFrontPagination"],["crosswordTable",""],[4,"ngFor","ngForOf"],["nzAlign","center",3,"potentian-letter","selected","click",4,"ngFor","ngForOf"],["nzAlign","center",3,"click"],["type","text","maxlength","1","nz-input","","nzBorderless","","nzSize","small",3,"disabled","ngModel","ngModelChange",4,"ngIf"],["type","text","maxlength","1","nz-input","","nzBorderless","","nzSize","small",3,"disabled","ngModel","ngModelChange"],[1,"definition-table",3,"nzData","nzScroll","nzShowPagination","nzFrontPagination"],["definitionsTable",""]],template:function(o,e){1&o&&(t.TgZ(0,"section",0)(1,"nz-select",1),t.YNc(2,J,1,2,"nz-option",2),t.ALo(3,"async"),t.qZA(),t.YNc(4,L,2,1,"nz-select",3),t.qZA(),t.YNc(5,E,8,7,"section",4),t.TgZ(6,"section",5),t.YNc(7,W,5,8,"nz-table",6),t.ALo(8,"async"),t.YNc(9,H,8,6,"nz-table",7),t.ALo(10,"async"),t.qZA()),2&o&&(t.Q6J("formGroup",e.filterForm),t.xp6(2),t.Q6J("ngForOf",t.lcZ(3,6,e.themes$)),t.xp6(2),t.Q6J("ngIf",e.selectedThemeId),t.xp6(1),t.Q6J("ngIf",e.filtersPresent),t.xp6(2),t.Q6J("ngIf",t.lcZ(8,8,e.crosswordMatrix$)),t.xp6(2),t.Q6J("ngIf",t.lcZ(10,10,e.selectedCrossword$)))},dependencies:[m.sg,m.O5,z.Ip,z.Vq,a.Fj,a.JJ,a.JL,a.nD,a.sg,a.u,a.On,y.Zp,p.N8,p.Uo,p._C,p.Om,p.p0,p.$Z,p.UX,T.ix,Z.w,U.dQ,m.Ov,$.a],styles:["[_nghost-%COMP%]{display:flex;flex-direction:column;height:100%}.themes[_ngcontent-%COMP%]{display:flex;align-items:center;gap:16px}nz-select[_ngcontent-%COMP%]{width:220px}.crossword-header[_ngcontent-%COMP%]{display:flex;align-items:center;gap:16px;margin-top:16px}.crossword-header[_ngcontent-%COMP%]   h2[_ngcontent-%COMP%]{margin:0}.crossword-header[_ngcontent-%COMP%]   nz-input-group[_ngcontent-%COMP%]{width:166px}.crossword-header[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]{margin-left:auto}.crossword-content[_ngcontent-%COMP%]{overflow:auto;margin-top:16px}.crossword-content[_ngcontent-%COMP%]   nz-table.crossword-table[_ngcontent-%COMP%]{margin-bottom:16px}.crossword-content[_ngcontent-%COMP%]   nz-table.crossword-table[_ngcontent-%COMP%]   td[_ngcontent-%COMP%]{padding:4px;height:31px;border:none;border-collapse:collapse}.crossword-content[_ngcontent-%COMP%]   nz-table.crossword-table[_ngcontent-%COMP%]   td.potentian-letter[_ngcontent-%COMP%]{border:1px solid rgb(104,104,104);background-color:#f0f0f0}.crossword-content[_ngcontent-%COMP%]   nz-table.crossword-table[_ngcontent-%COMP%]   td.potentian-letter.selected[_ngcontent-%COMP%]{background-color:#4aa8ff}.crossword-content[_ngcontent-%COMP%]   nz-table.crossword-table[_ngcontent-%COMP%]   td[_ngcontent-%COMP%]   input[_ngcontent-%COMP%]{padding:0;text-align:center;text-transform:uppercase}.crossword-content[_ngcontent-%COMP%]   nz-table.definition-table[_ngcontent-%COMP%]   td.selected-definition[_ngcontent-%COMP%]{background-color:#4aa8ff}"]}),f=(0,v.gn)([(0,w.c)()],f);var j=l(8176),R=l(1102),X=l(9444);const K=[{path:"",component:f}];let k=(()=>{class n{}return n.\u0275fac=function(e){return new(e||n)},n.\u0275mod=t.oAB({type:n}),n.\u0275inj=t.cJS({imports:[m.ez,j.Bz.forChild(K),z.LV,R.PV,a.UX,a.u5,y.o7,p.HQ,T.sL,X.D]}),n})()}}]);