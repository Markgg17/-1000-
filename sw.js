const CACHE="markgg-nexus-v3";
const FILES=["./","./index.html","./404.html","./manifest.webmanifest","./icon.svg","./apps/neon-arena/index.html","./apps/pixel-studio/index.html","./apps/creator-studio/index.html","./apps/3d-lab/index.html","./apps/code-lab/index.html","./apps/color-forge/index.html"];
self.addEventListener("install",event=>event.waitUntil(caches.open(CACHE).then(c=>c.addAll(FILES)).then(()=>self.skipWaiting())));
self.addEventListener("activate",event=>event.waitUntil(Promise.all([caches.keys().then(keys=>Promise.all(keys.filter(k=>k!==CACHE&&k.startsWith("markgg-nexus-")).map(k=>caches.delete(k)))),self.clients.claim()])));
self.addEventListener("fetch",event=>{
const req=event.request;if(req.method!=="GET"||new URL(req.url).origin!==self.location.origin)return;
event.respondWith(fetch(req).then(res=>{if(res.ok){const cp=res.clone();event.waitUntil(caches.open(CACHE).then(c=>c.put(req,cp)))}return res}).catch(async()=>{const cached=await caches.match(req);if(cached)return cached;if(req.mode==="navigate")return caches.match("./404.html");return Response.error()}));
});