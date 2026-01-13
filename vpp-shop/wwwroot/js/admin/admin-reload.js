/**
 * Reload partial view vào 1 container bất kỳ
 * @param {string} url - link controller trả về PartialView
 * @param {string} targetId - id của div cần reload
 */
function reloadPartial(url, targetId) {
    $("#" + targetId).load(url);
}
