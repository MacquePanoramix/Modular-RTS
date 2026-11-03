import json, sys
R = json.load(open(sys.argv[1]))
only = sys.argv[2].split('|') if len(sys.argv) > 2 else None
for r in R:
    if only and r['body'] not in only:
        continue
    rel = ''
    if 'release_sp_margin' in r:
        rel = ' | let go: sp %+4.0fmm r %s' % (1000*r['release_sp_margin'], ('%.2f' % r['release_r']) if r['release_r'] < 1e9 else 'inf')
    rr = ('%5.2f' % r['r']) if r['r'] < 1e9 else '  inf'
    print('%-8s %-70s c%7.3f r%s %-10s h%4.2f t%4.2f sp%+5.0f fr%+5.0f e%4.1f p%4.1f%s' % (
        r['body'][:8], r['stance'][:70], r['cost'], rr, str(r['worst'])[:10], r.get('hands_share', 0), r.get('on_thighs', 0), 1000*r['sp_margin'], 1000*r['fr_depth'], r['contact_err_mm'], r['penetration_mm'], rel))
