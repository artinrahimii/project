#!/usr/bin/env python3
"""Synchronize post-acceptance legacy reviews in a complete local Git checkout."""
from __future__ import annotations
import json, os, re, subprocess, sys, time
from datetime import datetime, timezone
from pathlib import Path

ROOT=Path.cwd()
UPDATE=Path("update/Step two/project-graph.md")
LEGACY=Path("step three/project-graph.md")
LEDGER=Path("update/Step two/legacy-task-reconciliation.json")
LEGACY_MD=Path("update/Step two/legacy-task-reconciliation.md")
REPORT=Path("update/Step three/UPD-S1-014/execution-report.md")
EVIDENCE=Path("update/Step three/UPD-S1-014/evidence/discovery-selector-A008.md")
TASK="UPD-S1-014"
ATTEMPT="UPD-S1-014-20261111-A008"
CODE_SHA="f9f4cc8ad57964dedaca985ccdc679fee5eb7718"
REVIEW_SHA="3e786ad83028cd3710e137243bb66963794bf198"
IDS=("T0116","T0117","T0118","T0121","T0122","T0123","T0124","T0127")
WORKFLOW=".github/workflows/reconcile-legacy-post-acceptance.yml"

def git(*args,check=True):
    p=subprocess.run(["git",*args],cwd=ROOT,text=True,stdout=subprocess.PIPE,stderr=subprocess.STDOUT)
    if check and p.returncode: raise RuntimeError(p.stdout)
    return p.stdout.strip()

def now():
    return datetime.now(timezone.utc).isoformat(timespec="seconds").replace("+00:00","Z")

def atomic(path,content):
    path=ROOT/path
    path.parent.mkdir(parents=True,exist_ok=True)
    tmp=path.with_name(path.name+".tmp")
    tmp.write_text(content,encoding="utf-8",newline="")
    tmp.replace(path)

def read_graph(path):
    raw=(ROOT/path).read_text(encoding="utf-8")
    fence=chr(96)*3
    marker=fence+"json"
    start=raw.find(marker)
    if start<0: raise RuntimeError("JSON fence missing in "+str(path))
    start+=len(marker)
    while start<len(raw) and raw[start] in " \t\r\n": start+=1
    end=raw.find(fence,start)
    if end<0: raise RuntimeError("JSON fence is not closed in "+str(path))
    return raw,json.loads(raw[start:end].strip()),start,end

def write_graph(path,raw,start,end,value,compact=False):
    data=json.dumps(value,ensure_ascii=False,separators=(",",":") if compact else None,indent=None if compact else 2)
    atomic(path,raw[:start]+data+raw[end:])

def add(items,value):
    if value not in items: items.append(value)

def version():
    m=re.fullmatch(r"V 3\.1\.(\d+)",git("log","-1","--format=%s"))
    if not m: raise RuntimeError("Latest main commit message is not V 3.1.X")
    return "V 3.1."+str(int(m.group(1))+1)

def set_progress(graph,stamp):
    counts={}
    for n in graph["nodes"]: counts[n["status"]]=counts.get(n["status"],0)+1
    total=len(graph["nodes"]); done=counts.get("done",0)
    graph["progress_snapshot"]={**graph.get("progress_snapshot",{}),"as_of":stamp,"total_tasks":total,
        "not_started":counts.get("not_started",0),"in_progress":counts.get("in_progress",0),
        "done":done,"blocked":counts.get("blocked",0),"verified_completion_percentage":done*100.0/total,
        "calculation":f"{done} verified done update tasks / {total} total update tasks = {done*100.0/total:.2f}%. UPD-S1-014 awaits exact-head synchronization validation; UPD-S1-020 remains blocked on physical hardware and a loaded local model."}

def push(parent,paths):
    git("fetch","origin","main")
    if git("rev-parse","refs/remotes/origin/main")!=parent: raise RuntimeError("Remote main moved; refusing push")
    git("add","--",*[str(p) for p in paths])
    if subprocess.run(["git","diff","--cached","--quiet"],cwd=ROOT).returncode==0: raise RuntimeError("Empty commit refused")
    msg=version()
    git("config","user.name","github-actions[bot]")
    git("config","user.email","41898282+artinrahimii@users.noreply.github.com")
    git("commit","-m",msg)
    sha=git("rev-parse","HEAD")
    git("fetch","origin","main")
    if git("rev-parse","refs/remotes/origin/main")!=parent: raise RuntimeError("Remote main changed before push")
    git("push","origin","HEAD:refs/heads/main")
    if git("ls-remote","origin","refs/heads/main").split()[0]!=sha: raise RuntimeError("Remote main SHA mismatch")
    print("Online main verified:",msg,sha)
    return sha

def quality(target):
    token=os.environ.get("GH_TOKEN") or os.environ.get("GITHUB_TOKEN")
    if not token: return {"success":False,"reason":"GitHub Actions token unavailable"}
    env=os.environ.copy(); env["GH_TOKEN"]=token
    p=subprocess.run(["gh","workflow","run",WORKFLOW,"--ref","main"],cwd=ROOT,env=env,text=True,stdout=subprocess.PIPE,stderr=subprocess.STDOUT)
    print(p.stdout)
    if p.returncode: return {"success":False,"reason":p.stdout}
    for _ in range(180):
        q=subprocess.run(["gh","run","list","--workflow",WORKFLOW,"--branch","main","--event","workflow_dispatch","--limit","30","--json","databaseId,headSha,status,conclusion,event,url"],cwd=ROOT,env=env,text=True,stdout=subprocess.PIPE,stderr=subprocess.STDOUT)
        if q.returncode==0:
            try: runs=json.loads(q.stdout)
            except json.JSONDecodeError: runs=[]
            run=next((x for x in runs if x.get("headSha")==target and x.get("event")=="workflow_dispatch"),None)
            if run and run.get("status")=="completed":
                return {"success":run.get("conclusion")=="success","run_id":run.get("databaseId"),"url":run.get("url") or "","reason":run.get("conclusion")}
        time.sleep(5)
    return {"success":False,"reason":"Timeout waiting for exact-head quality"}

def sync():
    ur,ug,us,ue=read_graph(UPDATE)
    lr,lg,ls,le=read_graph(LEGACY)
    ledger=json.loads((ROOT/LEDGER).read_text(encoding="utf-8"))
    if len(lg["nodes"])!=230 or len(lg["edges"])!=422 or len(ledger["records"])!=168: raise RuntimeError("Unexpected legacy inventory")
    original_sha=git("hash-object",str(LEGACY)); original_size=(ROOT/LEGACY).stat().st_size
    if original_sha!="af686f7163322710e6c7dca87678ce5b2c345052": raise RuntimeError("Legacy source Git Blob SHA mismatch")
    nodes={n["id"]:n for n in lg["nodes"]}
    records={r["legacy_task_id"]:r for r in ledger["records"]}
    if len(records)!=168: raise RuntimeError("Duplicate reconciliation IDs")
    review_shas=set()
    for tid in IDS:
        path=Path(f"update/Step three/UPD-S1-008/evidence/post-acceptance-UPD-S1-014-{tid}.md")
        doc=(ROOT/path).read_text(encoding="utf-8")
        if f"# بازبینی پس از پذیرش UPD-S1-014 — {tid}" not in doc: raise RuntimeError("Review heading mismatch "+tid)
        if "decision پیشنهادی پس از بازبینی: unchanged" not in doc or "review_status: reviewed" not in doc or "status_changed: false" not in doc: raise RuntimeError("Review decision mismatch "+tid)
        if f"commit کد بررسی‌شده: {CODE_SHA}" not in doc: raise RuntimeError("Review code baseline mismatch "+tid)
        m=re.search(r"^- update_task_ids: (.+)$",doc,re.M)
        if not m: raise RuntimeError("Missing update_task_ids "+tid)
        update_ids=sorted(x.strip() for x in m.group(1).split(","))
        node=nodes.get(tid); rec=records.get(tid)
        if not node or not rec or not node.get("update_impact"): raise RuntimeError("Missing record/marker "+tid)
        marker=node["update_impact"]
        if update_ids!=sorted(rec["update_task_ids"]) or update_ids!=sorted(marker["update_task_ids"]): raise RuntimeError("Update mapping mismatch "+tid)
        if node["status"]!="done" or rec["baseline_status"]!="done" or marker["legacy_status_at_review_start"]!="done": raise RuntimeError("Baseline changed "+tid)
        review_sha=git("log","-1","--format=%H","--",str(path))
        review_shas.add(review_sha)
        rec["decision"]="unchanged"; rec["review_status"]="reviewed"; rec["status_changed"]=False; rec["review_commit_sha"]=review_sha
        add(rec["evidence"],str(path))
        marker["decision"]="unchanged"; marker["review_status"]="reviewed"; marker["legacy_status_changed"]=False
        marker["reason"]=f"Post-acceptance review after UPD-S1-014 found existing behavior and exact-head tests sufficient; baseline done and history remain unchanged. Evidence: {path}; review commit: {review_sha}."
    if review_shas!={REVIEW_SHA}: raise RuntimeError("Unexpected review evidence commit set: "+str(review_shas))
    git("merge-base","--is-ancestor",CODE_SHA,"HEAD")
    git("merge-base","--is-ancestor",REVIEW_SHA,"HEAD")
    totals={}
    for rec in ledger["records"]:
        node=nodes[rec["legacy_task_id"]]; marker=node["update_impact"]
        if node["status"]!=rec["baseline_status"] or marker["legacy_status_at_review_start"]!=rec["baseline_status"]: raise RuntimeError("Legacy baseline mismatch "+node["id"])
        if marker["decision"]!=rec["decision"] or marker["review_status"]!=rec["review_status"] or marker["legacy_status_changed"]!=rec["status_changed"]: raise RuntimeError("Ledger marker mismatch "+node["id"])
        if sorted(marker["update_task_ids"])!=sorted(rec["update_task_ids"]): raise RuntimeError("Mapping differs "+node["id"])
        totals[rec["decision"]]=totals.get(rec["decision"],0)+1
    if totals!={"unchanged":20,"re_review_required":148}: raise RuntimeError("Decision totals differ: "+str(totals))

    write_graph(LEGACY,lr,ls,le,lg,True)
    new_sha=git("hash-object",str(LEGACY)); new_size=(ROOT/LEGACY).stat().st_size
    atomic(LEDGER,json.dumps(ledger,ensure_ascii=False,indent=2)+"\n")
    narrative=(ROOT/LEGACY_MD).read_text(encoding="utf-8")
    heading=f"## بازبینی تکمیلی پس از پذیرش UPD-S1-014 — {version()}"
    if heading not in narrative:
        narrative+=f"\n\n{heading}\n\n- هشت سند پس از پذیرش برای T0116، T0117، T0118، T0121، T0122، T0123، T0124 و T0127 بررسی شدند؛ تصمیم هر هشت unchanged است.\n- وضعیت پایه done و تاریخچهٔ وظایف قدیمی حفظ شد.\n- مجموع reconciliation: 168 reviewed، صفر pending؛ 20 unchanged و 148 re_review_required.\n"
    atomic(LEGACY_MD,narrative)

    node=next(n for n in ug["nodes"] if n["id"]==TASK)
    node["status"]="in_progress"; node["status_fa"]="در حال اجرا"
    stamp=now()
    attempt=next((a for a in node["execution_history"] if a["attempt_id"]==ATTEMPT),None)
    if attempt is None:
        attempt={"attempt_id":ATTEMPT,"started_at":stamp,"ended_at":stamp,"result":"partial","summary":"Eight post-acceptance decisions synchronized in a full local Git checkout; exact-head CI pending.","changed_files":[],"commit_sha":None,"tests":[],"failure_details":None,"blocked_reason":None,"reopened_reason":"The large-blob connector write limit was replaced with a full Git checkout on the GitHub Actions runner.","contract_changes":[],"follow_up":"Run exact-head quality before promoting to done.","device_and_host_details_redacted":"GitHub Actions runner only; no physical device used.","model_revision_and_sha256":"No local inference executed.","pre_upgrade_post_upgrade_snapshot":"Not applicable; tracked by UPD-S1-021."}
        node["execution_history"].append(attempt)
    changed=[LEGACY,LEDGER,LEGACY_MD,UPDATE,REPORT,EVIDENCE]
    attempt["ended_at"]=stamp; attempt["result"]="partial"; attempt["commit_sha"]=None; attempt["changed_files"]=[str(p) for p in changed]; attempt["tests"]=[]
    attempt["failure_details"]=None; attempt["blocked_reason"]=None; attempt["follow_up"]="Run exact-head quality and promote only if all six steps pass."
    sm=node["execution_summary"]; sm["attempt_count"]=len(node["execution_history"])
    sm["failed_attempt_count"]=sum(1 for x in node["execution_history"] if x.get("result")=="failed")
    sm["last_attempt_id"]=ATTEMPT; sm["last_attempt_result"]="partial"; sm["blocked_reason"]=None; sm["follow_up"]=attempt["follow_up"]
    for p in changed: add(sm["changed_files"],str(p))
    for p in changed:
        if p!=UPDATE: add(node["actual_output_paths"],str(p))
    add(node["evidence"],str(EVIDENCE)); add(sm["related_commits"],REVIEW_SHA)
    stamp=now(); ug["updated_at"]=stamp; ug["commit_intent"]=version(); set_progress(ug,stamp)
    write_graph(UPDATE,ur,us,ue,ug)
    report=(ROOT/REPORT).read_text(encoding="utf-8")
    section="## تلاش A008 — همگام‌سازی امن گراف legacy و ledger"
    if section in report: report=report.split(section,1)[0].rstrip()
    report+=f"\n\n{section}\n\n- کد فنی: {CODE_SHA} — V 3.1.191؛ اسناد بازبینی: {REVIEW_SHA} — V 3.1.192.\n- Blob SHA اصلی گراف legacy: {original_sha}؛ اندازه {original_size:,} بایت UTF-8.\n- Blob SHA پس از sync: {new_sha}؛ اندازه {new_size:,} بایت UTF-8.\n- فایل کامل با Git محلی روی runner اصلاح شد؛ 168 رکورد و marker بدون اختلاف، 20 unchanged و 148 re_review_required.\n- CI دقیق commit sync در گام بعدی اجرا می‌شود؛ تا قبل از موفقیت، task تکمیل‌شده نیست.\n"
    atomic(REPORT,report)
    atomic(EVIDENCE,f"# شواهد UPD-S1-014 — تلاش A008\n\n- کد: {CODE_SHA}; review commit: {REVIEW_SHA}.\n- Blob SHA legacy قبل: {original_sha}; اندازه {original_size:,} bytes.\n- Blob SHA legacy بعد: {new_sha}; اندازه {new_size:,} bytes.\n- روش نوشتن: full git checkout روی runner؛ 230 nodes، 422 edges، 168 markers.\n- reconciliation: 168 reviewed، صفر pending؛ 20 unchanged، 148 re_review_required.\n- هیچ status پایه یا history قدیمی تغییر نکرد. E2E فیزیکی و inference مدل واقعی اجرا نشده‌اند.\n- exact-head CI هنوز pending است.\n")
    _,old2,_,_=read_graph(LEGACY); _,update2,_,_=read_graph(UPDATE)
    if len(old2["nodes"])!=230 or len(update2["nodes"])!=24 or len(update2["edges"])!=80: raise RuntimeError("Post-write parse failed")
    return changed

def commit_paths(parent,paths):
    git("fetch","origin","main")
    if git("rev-parse","refs/remotes/origin/main")!=parent: raise RuntimeError("main changed")
    git("add","--",*[str(p) for p in paths])
    if subprocess.run(["git","diff","--cached","--quiet"],cwd=ROOT).returncode==0: raise RuntimeError("Empty commit refused")
    msg=version(); git("config","user.name","github-actions[bot]"); git("config","user.email","41898282+artinrahimii@users.noreply.github.com")
    git("commit","-m",msg); sha=git("rev-parse","HEAD")
    git("fetch","origin","main")
    if git("rev-parse","refs/remotes/origin/main")!=parent: raise RuntimeError("main changed before push")
    git("push","origin","HEAD:refs/heads/main")
    if git("ls-remote","origin","refs/heads/main").split()[0]!=sha: raise RuntimeError("Online SHA mismatch")
    print("Verified online",msg,sha)
    return sha

def dispatch_quality(target):
    env=os.environ.copy(); env["GH_TOKEN"]=env.get("GITHUB_TOKEN","")
    if not env["GH_TOKEN"]: return {"success":False,"reason":"GITHUB_TOKEN unavailable"}
    p=subprocess.run(["gh","workflow","run",WORKFLOW,"--ref","main"],cwd=ROOT,env=env,text=True,stdout=subprocess.PIPE,stderr=subprocess.STDOUT)
    print(p.stdout)
    if p.returncode: return {"success":False,"reason":p.stdout}
    for _ in range(180):
        q=subprocess.run(["gh","run","list","--workflow",WORKFLOW,"--branch","main","--event","workflow_dispatch","--limit","30","--json","databaseId,headSha,status,conclusion,event,url"],cwd=ROOT,env=env,text=True,stdout=subprocess.PIPE,stderr=subprocess.STDOUT)
        if q.returncode==0:
            try: runs=json.loads(q.stdout)
            except json.JSONDecodeError: runs=[]
            found=next((r for r in runs if r.get("headSha")==target and r.get("event")=="workflow_dispatch"),None)
            if found and found.get("status")=="completed":
                return {"success":found.get("conclusion")=="success","run_id":found.get("databaseId"),"url":found.get("url") or "","reason":found.get("conclusion")}
        time.sleep(5)
    return {"success":False,"reason":"Timed out waiting for exact-head quality"}

def finalize(sync_sha,run):
    raw,g,s,e=read_graph(UPDATE); n=next(x for x in g["nodes"] if x["id"]==TASK)
    a=next(x for x in n["execution_history"] if x["attempt_id"]==ATTEMPT)
    cmds=["python -m unittest discover -s tests -v","python -m compileall -q tools tests",
      "python tools/graph_guard.py step three/project-graph.md",
      "python tools/graph_guard.py step three/project-graph.md --repo-root .",
      "python tools/graph_guard.py update/Step two/project-graph.md",
      "python tools/graph_guard.py update/Step two/project-graph.md --repo-root ."]
    a["result"]="passed"; a["commit_sha"]=sync_sha; a["ended_at"]=now()
    a["tests"]=[{"name":c,"command":c,"result":"passed","run_id":run["run_id"],"url":run["url"],"summary":"Passed on exact synchronization commit."} for c in cmds]
    a["tests"].append({"name":"Exact-head quality gate","command":"workflow_dispatch on "+sync_sha,"result":"passed","run_id":run["run_id"],"url":run["url"],"summary":"All six quality steps passed."})
    a["summary"]="Eight post-acceptance decisions synchronized; exact-head quality passed; all 168 ledger and marker records match with baseline statuses and histories preserved."
    n["status"]="done"; n["status_fa"]=next((x.get("status_fa") for x in g["nodes"] if x.get("status")=="done" and x.get("status_fa")),"تکمیل‌شده")
    sm=n["execution_summary"]; sm["last_attempt_id"]=ATTEMPT; sm["last_attempt_result"]="passed"; sm["blocked_reason"]=None
    sm["attempt_count"]=len(n["execution_history"]); sm["failed_attempt_count"]=sum(1 for x in n["execution_history"] if x.get("result")=="failed")
    sm["follow_up"]="Continue in conservative order with UPD-S1-011 and UPD-S1-015. UPD-S1-020 remains blocked on physical hardware."
    add(sm["tests_passed"],f"Exact-head quality passed on {sync_sha}; run {run['run_id']}")
    add(sm["related_commits"],sync_sha)
    report=(ROOT/REPORT).read_text(encoding="utf-8")
    sync_commit_message=git("show","-s","--format=%s",sync_sha)
    report+=f"\n\n## CI دقیق A008 — {sync_commit_message}\n\n- Commit sync: {sync_sha}.\n- Quality Gate: PASS؛ run {run['run_id']} — {run['url']}.\n- تمام شش مرحلهٔ Python tests، compileall و graph_guard برای هر دو گراف موفق شدند.\n- reconciliation: 168 reviewed، صفر pending؛ 20 unchanged و 148 re_review_required.\n"
    atomic(REPORT,report)
    ev=(ROOT/EVIDENCE).read_text(encoding="utf-8")+f"\n\nExact-head CI passed on {sync_sha}; run {run['run_id']} — {run['url']}.\n"
    atomic(EVIDENCE,ev)
    stamp=now(); g["updated_at"]=stamp; g["commit_intent"]=version(); set_progress(g,stamp); write_graph(UPDATE,raw,s,e,g)
    return [UPDATE,REPORT,EVIDENCE]

def note_failure(sync_sha,run):
    raw,g,s,e=read_graph(UPDATE); n=next(x for x in g["nodes"] if x["id"]==TASK)
    a=next(x for x in n["execution_history"] if x["attempt_id"]==ATTEMPT)
    reason=str(run.get("reason") or "Exact-head quality failed.")
    a["result"]="failed"; a["commit_sha"]=sync_sha; a["ended_at"]=now(); a["failure_details"]=reason
    a["tests"]=[{"name":"Exact-head quality gate","command":"workflow_dispatch on "+sync_sha,"result":"failed","run_id":run.get("run_id"),"url":run.get("url"),"summary":reason}]
    a["summary"]="The synchronization commit was pushed, but exact-head quality failed; task remains incomplete."
    sm=n["execution_summary"]; sm["last_attempt_id"]=ATTEMPT; sm["last_attempt_result"]="failed"
    sm["attempt_count"]=len(n["execution_history"]); sm["failed_attempt_count"]=sum(1 for x in n["execution_history"] if x.get("result")=="failed")
    sm["follow_up"]="Inspect exact failing CI step, correct it, and rerun validation."
    stamp=now(); g["updated_at"]=stamp; g["commit_intent"]=version(); set_progress(g,stamp); write_graph(UPDATE,raw,s,e,g)
    report=(ROOT/REPORT).read_text(encoding="utf-8")+f"\n\nExact-head CI failed for {sync_sha}: {reason}. UPD-S1-014 remains in_progress.\n"
    atomic(REPORT,report)
    return [UPDATE,REPORT]

def reopen(final_sha,run):
    raw,g,s,e=read_graph(UPDATE); n=next(x for x in g["nodes"] if x["id"]==TASK)
    stamp=now(); reason=str(run.get("reason") or "Final exact-head quality failed.")
    n["status"]="in_progress"; n["status_fa"]="در حال اجرا"
    n["execution_history"].append({"attempt_id":"UPD-S1-014-20261111-A009","started_at":stamp,"ended_at":stamp,"result":"failed",
      "summary":"Final committed graph state failed exact-head quality; task reopened.",
      "changed_files":[str(UPDATE),str(REPORT)],"commit_sha":final_sha,
      "tests":[{"name":"Final exact-head quality","command":"workflow_dispatch on "+final_sha,"result":"failed","run_id":run.get("run_id"),"url":run.get("url"),"summary":reason}],
      "failure_details":reason,"blocked_reason":None,"reopened_reason":"Final graph state did not pass exact-head quality.",
      "contract_changes":[],"follow_up":"Fix exact failing validation and rerun quality.",
      "device_and_host_details_redacted":"GitHub Actions runner only; no physical product device.",
      "model_revision_and_sha256":"No real inference performed.","pre_upgrade_post_upgrade_snapshot":"Not applicable; tracked by UPD-S1-021."})
    sm=n["execution_summary"]; sm["attempt_count"]=len(n["execution_history"])
    sm["failed_attempt_count"]=sum(1 for x in n["execution_history"] if x.get("result")=="failed")
    sm["last_attempt_id"]="UPD-S1-014-20261111-A009"; sm["last_attempt_result"]="failed"; sm["reopened_reason"]=reason
    add(sm["related_commits"],final_sha)
    stamp=now(); g["updated_at"]=stamp; g["commit_intent"]=version(); set_progress(g,stamp); write_graph(UPDATE,raw,s,e,g)
    atomic(REPORT,(ROOT/REPORT).read_text(encoding="utf-8")+f"\n\nA009: final quality failed for {final_sha}: {reason}. Task reopened.\n")
    return [UPDATE,REPORT]

def main():
    event=os.environ.get("GITHUB_EVENT_NAME")
    if event=="workflow_dispatch": return
    if event!="push": raise RuntimeError("Only push events may perform the reconciliation write.")
    if os.environ.get("GITHUB_REF")!="refs/heads/main": raise RuntimeError("Reconciliation writes are restricted to refs/heads/main.")
    parent=git("rev-parse","HEAD")
    if os.environ.get("GITHUB_SHA") and parent!=os.environ["GITHUB_SHA"]: raise RuntimeError("Checkout SHA differs from push event")
    git("fetch","origin","main")
    if git("rev-parse","refs/remotes/origin/main")!=parent: raise RuntimeError("Remote main moved")
    version()  # Validate the current V 3.1.X sequence before touching files.
    paths=sync()
    sync_sha=commit_paths(parent,paths)
    first=dispatch_quality(sync_sha)
    if not first.get("success"):
        failure_paths=note_failure(sync_sha,first)
        failure_sha=commit_paths(sync_sha,failure_paths)
        print("A008 remains incomplete; failure recorded in",failure_sha)
        raise RuntimeError(str(first))
    final_paths=finalize(sync_sha,first)
    final_sha=commit_paths(sync_sha,final_paths)
    second=dispatch_quality(final_sha)
    if second.get("success"):
        print("UPD-S1-014 done. Sync commit",sync_sha,"final commit",final_sha,"final run",second.get("run_id"))
        return
    recovery_paths=reopen(final_sha,second)
    recovery_sha=commit_paths(final_sha,recovery_paths)
    print("UPD-S1-014 reopened in",recovery_sha)
    raise RuntimeError(str(second))

if __name__=="__main__":
    main()
