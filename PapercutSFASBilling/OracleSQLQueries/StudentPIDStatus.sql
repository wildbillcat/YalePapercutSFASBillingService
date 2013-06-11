SELECT
 syvyids_pidm, stvests_desc
FROM
  syvyids, sfbetrm, stvests
WHERE
      syvyids_pidm = sfbetrm_pidm
  AND sfbetrm_term_code = '201301'
  AND sfbetrm_ests_code = stvests_code
  AND syvyids_netid = 'rch42';

