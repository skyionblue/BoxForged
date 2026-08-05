# BoxForged — Website Deploy

**Live site:** https://theboxforged.com
**Server web root:** `/var/www/theboxforged.com/`

---

## Deploy

### 1. Create the tarball (run from the repo root)

```bash
tar -czf website-deploy.tar.gz -C website index.html assets/ privacy/
```

### 2. Upload to the server

Use the Google Cloud SSH browser upload button, or `scp`:

```bash
scp website-deploy.tar.gz user@yourserver:~/
```

### 3. Extract on the server

```bash
sudo tar -xzvf website-deploy.tar.gz -C /var/www/theboxforged.com/
```

The `-v` flag prints every file as it extracts so you can confirm what landed.

### 4. Reload nginx

```bash
sudo nginx -s reload
```

---

## Toggling Community Submissions

The submission form on the site is controlled by a single flag in `index.html`.

**To disable** (hide the form, show a "coming soon" message):
```js
const SUBMISSIONS_OPEN = false;
```

**To enable** (show the form):
```js
const SUBMISSIONS_OPEN = true;
```

The flag is at the very top of the `<script>` block near the bottom of `index.html` — search for `SUBMISSIONS_OPEN` to find it instantly. Change the value, deploy, and the site updates immediately. No other changes needed.

When `false`: the form is hidden and a "Community submissions are opening soon" message appears in its place.
When `true`: the form is visible and fully functional (assuming the nginx + n8n backend is wired — see Pending pre-release steps below).

---

## Verify

```bash
curl -s https://theboxforged.com | grep -i "your search term"
```

---

## Pending pre-release steps

These changes are built and committed but **not yet deployed**. Complete them when the site goes into active release mode.

### 1. Deploy the updated nginx config

The `theboxforged.com.conf` now includes an `/api/submit` proxy location that routes the community form to n8n and applies rate limiting. Before deploying it:

**Add the rate-limit zone to the nginx.conf http {} block** (not the site conf — it must live in `http {}`):

```nginx
# /etc/nginx/nginx.conf  — inside the http {} block
limit_req_zone $binary_remote_addr zone=uh_submit:10m rate=3r/m;
```

Then place the updated site conf and reload:

```bash
sudo cp theboxforged.com.conf /etc/nginx/sites-available/theboxforged.com
sudo nginx -t          # confirm no syntax errors
sudo nginx -s reload
```

The proxy target is `http://127.0.0.1:5678/webhook/boxforged-submissions` (n8n default port). Change the host/port in the conf if n8n runs elsewhere.

### 2. Deploy the n8n workflows

Import in this order — the error handler's workflow ID is needed by the other two:

1. Import `n8n-workflows/boxforged-error-handler.json` → activate → copy the n8n-assigned workflow ID
2. Open `n8n-workflows/boxforged-website-submissions.json` and replace `YOUR_ERROR_HANDLER_WORKFLOW_ID` with that ID, then import and wire credentials:
   - `YOUR_SUBMISSIONS_SHEET_ID` — Google Sheet ID from its URL (`spreadsheets/d/<ID>/`)
   - `GOOGLE_SHEETS_CREDENTIAL_ID` — Google Sheets OAuth2 credential
   - `GMAIL_CREDENTIAL_ID` — Gmail OAuth2 credential
3. Import `n8n-workflows/boxforged-social-publisher.json` and wire:
   - `YOUR_SOCIAL_SHEET_ID` — Google Sheet for scheduled posts
   - `YOUR_IG_PAGE_ID` — Instagram Business Account numeric ID
   - `YOUR_YOUTUBE_CHANNEL_ID` — YouTube channel ID
   - `YOUR_TWITCH_BROADCASTER_ID`, `YOUR_TWITCH_MODERATOR_ID`, `YOUR_TWITCH_CLIENT_ID`, `YOUR_TWITCH_BEARER_TOKEN`
   - `FACEBOOK_GRAPH_CREDENTIAL_ID`, `TIKTOK_OAUTH2_CREDENTIAL_ID`, `YOUTUBE_OAUTH2_CREDENTIAL_ID`
   - `YOUR_ERROR_HANDLER_WORKFLOW_ID` (same ID from step 1)

Activate submissions workflow first, then social publisher.

### 3. Wire the website form webhook URL

After the submissions workflow is active, the form at `/api/submit` routes through nginx — no URL change needed in the HTML. Confirm end-to-end with:

```bash
curl -s -X POST https://theboxforged.com/api/submit \
  -H "Content-Type: application/json" \
  -d '{"name":"Test","email":"test@example.com","idea":"Test idea","website":""}'
# Expected: {"success":true,"message":"Submission received! We read every idea."}
```

### 4. Create the Google Sheets

**Social posts sheet** — one tab named `social-posts`, columns:
`Scheduled Date` | `Content` | `Image URL` | `Instagram` | `TikTok` | `YouTube` | `Twitch` | `Status`

**Submissions sheet** — one tab named `submissions`, columns:
`Timestamp` | `Name` | `Email` | `Idea` | `Status`

### Spam protection summary (already built in)

| Layer | What it does |
|---|---|
| nginx rate limit | 3 req/min per IP, burst 5 — returns 429 before n8n sees it |
| nginx proxy | n8n URL never exposed publicly — form posts to `/api/submit` |
| Honeypot field | Hidden `website` input; bots fill it, silently accepted but not saved |
