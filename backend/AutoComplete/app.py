from flask import Flask, jsonify, request
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
import time
from flask_cors import CORS
import requests
import json

app = Flask(__name__)
CORS(app)


@app.get("/autocomplete")
def get_title():
    
    url = request.args.get("url", "https://avize.distributie-energie.ro/login")

    chrome_options = Options()
    chrome_options.add_argument("--start-maximized")
    chrome_options.add_argument("--no-sandbox")
    chrome_options.add_argument("--disable-dev-shm-usage")

    driver = webdriver.Chrome(options=chrome_options)

    try:
        driver.get(url)

        login(driver)

        return jsonify({
            "ok": True
        })

    except Exception as e:
        return jsonify({
            "ok": False,
            "error": str(e),
        }), 500

    finally:
        # comentează temporar dacă vrei să rămână browserul deschis
        # driver.quit()
        pass


def login(driver):

    wait = WebDriverWait(driver, 20)

    email_input = wait.until(
        EC.presence_of_element_located(
            (By.XPATH, "//input[@id='_com_liferay_login_web_portlet_LoginPortlet_login']")
        )
    )

    email_input.clear()
    email_input.send_keys("proiectare@ovinstalelectric.ro")

    password_input = wait.until(
        EC.presence_of_element_located(
            (By.XPATH, "//input[@id='_com_liferay_login_web_portlet_LoginPortlet_password']")
        )
    )

    password_input.clear()
    password_input.send_keys("Ovinstal2011!")

    login_button = wait.until(
        EC.element_to_be_clickable((By.XPATH, "//button[@type='submit']"))
    )

    login_button.click()

    wait.until(EC.url_changes(driver.current_url))

    print("Pagina după login încărcată:", driver.current_url)

    cookies = driver.get_cookies()
    print(cookies)

    cerere_btn = wait.until(EC.presence_of_element_located((By.XPATH, "//button[@class='dropdown-toggle text-nowrap btn btn-primary']")))
    cerere_btn.click()

    prosumator_btn=wait.until(EC.presence_of_element_located((By.XPATH, "//a[contains(text(),'ATR Prosumator')]")))
    prosumator_btn.click()


    iframe = wait.until(
        EC.presence_of_element_located((By.TAG_NAME, "iframe"))
    )
    driver.switch_to.frame(iframe)
    print("Form loaded")

    fields = driver.execute_script("""
    var data = {};
    document.querySelectorAll("#elo_wf_form input[type=hidden]").forEach(el => {
        data[el.name] = el.value;
    });
    return data;
    """)

    print(fields)

    url = "https://avize.distributie-energie.ro/ix-INAportal/plugin/de.elo.ix.plugin.proxy/wf/wf/store.jsp"

    params = {
    "lang": "en",
    "nonce": fields["nonce"],
    "ELO_TICKET": "de.elo.ix.client.ticket_from_cookie",
    }

    headers = {
        "Accept": "*/*",
        "Content-Type": "application/json",
        "Origin": "https://avize.distributie-energie.ro",
        "Referer": "https://avize.distributie-energie.ro/ix-INAportal/plugin/de.elo.ix.plugin.proxy/wf/wf/110_atr_prosumator.jsp?wfid=329210&nodeid=98&lang=en",
        "User-Agent": "Mozilla/5.0",
    }

    cookies = driver.get_cookies()

    session = requests.Session()

    for c in cookies:
     session.cookies.set(
        name=c["name"],
        value=c["value"],
        domain=c.get("domain"),
        path=c.get("path", "/")
    )

    payload={
  "ELO_TICKET": "de.elo.ix.client.ticket_from_cookie",
  "ELO_OBJID": fields["ELO_OBJID"],
  "ELO_FLOWNODE": fields["ELO_FLOWNODE"],
  "ELO_FLOWID": fields["ELO_FLOWID"],
  "ELO_NODEID": "98",
  "ELO_TEMPLATE": "110_atr_prosumator",
  "ELO_REFERER": "https%3A%2F%2Favize.distributie-energie.ro%2Fuser%2Fproiectare_ovinstalelectric_ro",
  "ELO_LAST_MODIFIED": fields["ELO_LAST_MODIFIED"],
  "ELO_LAST_MODIFIED_HASH": fields["ELO_LAST_MODIFIED_HASH"],
  "nonce": fields["nonce"],
  "lang": "en",
  "IX_GRP_NR_INREGISTRARE": "",
  "IX_GRP_DATA_INREGISTRARE": "",
  "IX_GRP_P_INSTAL_PROD": "≤ 400 kW",
  "IX_MAP_P_ABS_PROD": "≤ 100 kW",
  "IX_MAP_SISTEM_STOCARE": "Da",
  "IX_GRP_TIP_SOLICITANT_PF": "Persoana Fizica",
  "IX_MAP_CETATENIE": "Romana",
  "IX_GRP_CNP_SOLICITANT": "5030511055091",
  "IX_GRP_NUME_SOLICITANT": "POPESCU",
  "IX_GRP_PRENUME_SOLICITANT": "ION",
  "IX_GRP_COD_CAEN_SOLICITANT": "9801",
  "IX_GRP_NR_TELEFON_SOLICITANT": "",
  "IX_GRP_EMAIL_SOLICITANT": "",
  "IX_GRP_CONT_BANCAR_SOLICITANT": "",
  "IX_MAP_BANCA_SOLICITANT": "",
  "IX_MAP_SUCURSALA_SOLICITANT": "",
  "IX_MAP_FUNCTIE_SOLICITANT": "",
  "IX_GRP_DENUMIRE_SOCIETATE_SOLICITANT": "",
  "IX_GRP_CUI_SOLICITANT": "",
  "IX_GRP_NR_REGISTRU_COMERTULUI_SOLICITANT": "",
  "IX_MAP_CIF_SOLICITANT": "",
  "IX_GRP_JUDET_SOLICITANT": "BIHOR",
  "IX_GRP_DOMICILIU_SOLICITANT": "MUNICIPIU",
  "IX_GRP_LOCALITATE_SOLICITANT": "SANTANDREI",
  "IX_MAP_TEMP_TIP_LOCALITATE": "SAT",
  "IX_GRP_SAT_SOLICITANT": "AUSEU",
  "IX_MAP_TEMP_TIP_STRADA": "STRADA",
  "IX_GRP_STRADA_SOLICITANT": "PRINCIPALA",
  "IX_GRP_NR_STRADA_SOLICITANT": "2",
  "IX_GRP_BLOC_SOLICITANT": "",
  "IX_GRP_SCARA_SOLICITANT": "",
  "IX_GRP_ETAJ_SOLICITANT": "",
  "IX_GRP_APARTAMENT_SOLICITANT": "",
  "IX_GRP_COD_POSTAL_SOLICITANT": "417025",
  "IX_MAP_TARA_SOLICITANT": "",
  "IX_BLOB_TIP_OBIECTIV": "Activitati sezoniere",
  "IX_GRP_TARLA_OBIECTIV": "",
  "IX_GRP_PARCELA_OBIECTIV": "",
  "IX_GRP_LOT_OBIECTIV": "",
  "IX_GRP_NR_CADASTRAL_OBIECTIV": "",
  "IX_MAP_NR_CF_OBIECTIV": "",
  "IX_GRP_JUDET_OBIECTIV": "Bihor",
  "IX_GRP_TIP_AMPLASARE_OBIECTIV": "",
  "IX_GRP_LOCALITATE_OBIECTIV": "SANTANDREI",
  "IX_GRP_SAT_OBIECTIV": "SANTANDREI",
  "IX_GRP_STRADA_OBIECTIV": "SANTANDREI",
  "IX_GRP_NR_STRADA_OBIECTIV": "2",
  "IX_GRP_BLOC_OBIECTIV": "",
  "IX_GRP_SCARA_OBIECTIV": "",
  "IX_GRP_ETAJ_OBIECTIV": "",
  "IX_GRP_APARTAMENT_OBIECTIV": "",
  "IX_GRP_COD_POSTAL_OBIECTIV": "417197",
  "IX_MAP_SAP_COD_JUDET_OBIECTIV": "05",
  "IX_MAP_SAP_COD_COMUNA_OBIECTIV": "362",
  "IX_MAP_SAP_COD_LOCALITATE_OBIECTIV": "1601",
  "IX_MAP_SAP_COD_STRADA_OBIECTIV": "7705",
  "IX_GRP_TIP_CERERE_CONSULTANT": "Fara Imputernicit",
  "IX_GRP_CNP_CONSULTANT": "",
  "IX_GRP_NUME_CONSULTANT": "",
  "IX_GRP_PRENUME_CONSULTANT": "",
  "IX_GRP_EMAIL_CONSULTANT": "",
  "IX_GRP_NR_TELEFON_CONSULTANT": "",
  "IX_MAP_ACT_CONSULTANT": "",
  "IX_MAP_DATA_ACT_CONSULTANT": "",
  "IX_MAP_EMITATOR_ACT_CONSULTANT": "",
  "IX_GRP_DENUMIRE_CONSULTANT": "",
  "IX_GRP_CUI_CONSULTANT": "",
  "IX_GRP_J_CONSULTANT": "",
  "IX_GRP_CONT_BANCAR_CONSULTANT": "",
  "IX_GRP_COD_CAEN_CONSULTANT": "",
  "IX_MAP_IMPUTERNICIT_PJ_1": "PERSOANĂ JURIDICĂ: , cu domiciliul/ sediul in municipiul/orasul/comuna , satul , str. , nr. , bloc , ap. , scara , etaj , cod postal ,",
  "IX_MAP_IMPUTERNICIT_PJ_2": "jud. , nr. telefon , e-mail: , CUI/CIF",
  "IX_MAP_IMPUTERNICIT_PF_1": "PERSOANĂ FIZICĂ: ............................, cu domiciliul/ sediul in municipiul/orasul/comuna ................., satul ............., str. .................., nr. ...., bloc ......., ap. ....., scara ....., etaj ...., cod postal ......,",
  "IX_MAP_IMPUTERNICIT_PF_2": "jud. ................., nr. telefon ................, e-mail: ............................",
  "IX_GRP_JUDET_CONSULTANT": "",
  "IX_GRP_TIP_LOCALITATE_CONSULTANT": "",
  "IX_GRP_ORAS_CONSULTANT": "",
  "IX_MAP_TEMP_TIPC_LOCALITATE": "",
  "IX_GRP_SAT_CONSULTANT": "",
  "IX_MAP_TEMP_TIPC_STRADA": "",
  "IX_GRP_STRADA_CONSULTANT": "",
  "IX_GRP_NR_STR_CONSULTANT": "",
  "IX_GRP_BLOC_CONSULTANT": "",
  "IX_GRP_SCARA_CONSULTANT": "",
  "IX_GRP_ETAJ_CONSULTANT": "",
  "IX_GRP_APARTAMENT_CONSULTANT": "",
  "COD_POSTAL_CONSULTANT": "",
  "IX_MAP_TARA_CONSULTANT": "",
  "IX_GRP_DOMICILIU_CONSULTANT": "",
  "IX_GRP_LOCALITATE_CONSULTANT": "",
  "IX_MAP_TEMP_TIPC_CONSULTANT": "",
  "IX_GRP_NR_STRADA_CONSULTANT": "",
  "IX_GRP_COD_POSTAL_CONSULTANT": "",
  "IX_GRP_TIP_CONSUMATOR": "Casnic",
  "IX_GRP_TIP_LOC_CONSUM": "Existent",
  "IX_MAP_TIP_LOC_PRODUCERE": "Nou",
  "IX_GRP_TIP_CONSTRUCTIE": "Existenta",
  "IX_GRP_NECESITA_AFM": "Nu",
  "IX_MAP_TIP_LOC_CONSUM_EXISTENT": "Modificari tehnice in instalatia de utilizare fara depasirea puterii aprobate",
  "IX_GRP_NR_AVIZ_EXISTENT": "",
  "IX_GRP_DATA_AVIZ_EXISTENT": "",
  "IX_GRP_POD_EXISTENT": "2141241241241",
  "IX_GRP_MOD_ALIMENTARE_OBIECTIV": "",
  "IX_GRP_MOD_LOC_MASURARE_ENERGIE": "",
  "IX_MAP_DATE_PUNCT_DELIMITARE_EXISTENT": "",
  "IX_GRP_PUTERE_INSTAL_EXISTENT": "",
  "IX_MAP_PUTERE_INSTAL_EXISTENT_CONSUM": "",
  "IX_GRP_PUTERE_INSTAL_EXISTENT_CENTRALA": "",
  "IX_GRP_PUTERE_APROB_EXISTENT": "",
  "IX_MAP_PUTERE_APROB_EXISTENT_CONSUM": "",
  "IX_GRP_PUTERE_APROB_EXISTENT_CENTRALA": "",
  "IX_GRP_TENSIUNE_UTILIZARE_EXISTENT": "",
  "IX_GRP_TENSIUNE_UTILIZARE_CONSUM": "",
  "IX_GRP_TENSIUNE_UTILIZARE_CENTRALA": "",
  "IX_GRP_TIP_BRANSAMENT_SOLICITARE": "Monofazat (230V)",
  "IX_MAP_NR_GRUPURI_GENERATOR_ASIN": "",
  "IX_MAP_NR_GG_SIN": "",
  "IX_MAP_TIP_GRUP_ASIN": "",
  "IX_MAP_TIP_GG_SIN": "",
  "IX_MAP_UN_GRUP_ASIN": "",
  "IX_MAP_TENSIUNE_GG_SIN": "",
  "IX_MAP_GRUP_GENERATOR_PN_ASIN": "",
  "IX_MAP_PN_GG_SIN": "",
  "IX_MAP_PN_UG_TOTAL": "0,000",
  "IX_MAP_SN_UG_ASIN": "",
  "IX_MAP_SN_UG_SIN": "",
  "IX_MAP_SN_UG_TOTAL": "0,000",
  "IX_MAP_PI_TOTAL_ASIN": "",
  "IX_MAP_PI_TOTAL_SIN": "",
  "IX_MAP_PI_TOTAL_FINAL": "0,000",
  "IX_MAP_U_ASIN": "",
  "IX_MAP_U_SIN": "",
  "IX_MAP_U_TOTAL": "0,00",
  "IX_MAP_PMAX_GG_ASIN": "",
  "IX_MAP_PMAX_GG_SIN": "",
  "IX_MAP_PMAX_GG_TOTAL": "0,000",
  "IX_MAP_PMIN_GG_ASIN": "",
  "IX_MAP_PMIN_GG_SIN": "",
  "IX_MAP_PMIN_GG_TOTAL": "0,000",
  "IX_MAP_QMAX_ASIN": "",
  "IX_MAP_QMAX_SIN": "",
  "IX_MAP_QMAX_TOTAL": "0,000",
  "IX_MAP_QMIN_ASIN": "",
  "IX_MAP_QMIN_SIN": "",
  "IX_MAP_QMIN_TOTAL": "0,000",
  "IX_MAP_SEVAC_ASIN": "",
  "IX_MAP_SEVAC_SIN": "",
  "IX_MAP_SEVAC_TOTAL": "0,000",
  "IX_MAP_OBSERVATII_ASIN": "",
  "IX_MAP_OBSERVATII_SIN": "",
  "IX_MAP_NR_CRT_FOTO": "1",
  "IX_MAP_NR_PANOURI_FOTO1": "1",
  "IX_MAP_NR_PANOURI_FOTO2": "",
  "IX_MAP_NR_PANOURI_FOTO3": "",
  "IX_MAP_NR_PANOURI_FOTO4": "",
  "IX_MAP_NR_PANOURI_FOTO5": "",
  "IX_MAP_TIP_PANOU1": "asda",
  "IX_MAP_TIP_PANOU2": "",
  "IX_MAP_TIP_PANOU3": "",
  "IX_MAP_TIP_PANOU4": "",
  "IX_MAP_TIP_PANOU5": "",
  "IX_MAP_PI_PANOU_KW_FOTO1": "0,500",
  "IX_MAP_PI_PANOU_KW_FOTO2": "",
  "IX_MAP_PI_PANOU_KW_FOTO3": "",
  "IX_MAP_PI_PANOU_KW_FOTO4": "",
  "IX_MAP_PI_PANOU_KW_FOTO5": "",
  "IX_MAP_PI_PANOU_KW_FOTO_T": "0,500",
  "IX_MAP_PI_TOTAL_KW_FOTO1": "0,500",
  "IX_MAP_PI_TOTAL_KW_FOTO2": "0,000",
  "IX_MAP_PI_TOTAL_KW_FOTO3": "0,000",
  "IX_MAP_PI_TOTAL_KW_FOTO4": "0,000",
  "IX_MAP_PI_TOTAL_KW_FOTO5": "0,000",
  "IX_MAP_PI_TOTAL_KW_FOTO_T": "0,500",
  "IX_MAP_P_MAX_DEBITAT_PAN1": "",
  "IX_MAP_P_MAX_DEBITAT_PAN2": "",
  "IX_MAP_P_MAX_DEBITAT_PAN3": "",
  "IX_MAP_P_MAX_DEBITAT_PAN4": "",
  "IX_MAP_P_MAX_DEBITAT_PAN5": "",
  "IX_MAP_P_MAX_DEBITAT_PAN_T": "0,000",
  "IX_MAP_CAP_BATERII_FOTO1": "",
  "IX_MAP_CAP_BATERII_FOTO2": "",
  "IX_MAP_CAP_BATERII_FOTO3": "",
  "IX_MAP_CAP_BATERII_FOTO4": "",
  "IX_MAP_CAP_BATERII_FOTO5": "",
  "IX_MAP_CAP_BATERII_FOTO_T": "0,00",
  "IX_MAP_PI_TOTAL_PAN_INV1": "",
  "IX_MAP_PI_TOTAL_PAN_INV2": "",
  "IX_MAP_PI_TOTAL_PAN_INV3": "",
  "IX_MAP_PI_TOTAL_PAN_INV4": "",
  "IX_MAP_PI_TOTAL_PAN_INV5": "",
  "IX_MAP_PI_TOTAL_PAN_INV_T": "0,000",
  "IX_MAP_MODULE_OBS1": "",
  "IX_MAP_MODULE_OBS2": "",
  "IX_MAP_MODULE_OBS3": "",
  "IX_MAP_MODULE_OBS4": "",
  "IX_MAP_MODULE_OBS5": "",
  "IX_MAP_NR_CRT_INV": "1",
  "IX_MAP_NR_INVERTOARE_FOTO1": "1",
  "IX_MAP_NR_INVERTOARE_FOTO2": "",
  "IX_MAP_NR_INVERTOARE_FOTO3": "",
  "IX_MAP_NR_INVERTOARE_FOTO4": "",
  "IX_MAP_NR_INVERTOARE_FOTO5": "",
  "IX_MAP_TIP_INVERTOARE1": "adsa",
  "IX_MAP_TIP_INVERTOARE2": "",
  "IX_MAP_TIP_INVERTOARE3": "",
  "IX_MAP_TIP_INVERTOARE4": "",
  "IX_MAP_TIP_INVERTOARE5": "",
  "IX_MAP_TENSIUNE_INVERTOR_FOTO1": "0.2",
  "IX_MAP_TENSIUNE_INVERTOR_FOTO2": "",
  "IX_MAP_TENSIUNE_INVERTOR_FOTO3": "",
  "IX_MAP_TENSIUNE_INVERTOR_FOTO4": "",
  "IX_MAP_TENSIUNE_INVERTOR_FOTO5": "",
  "IX_MAP_PI_INVERTOARE_FOTO1": "4,000",
  "IX_MAP_PI_INVERTOARE_FOTO2": "",
  "IX_MAP_PI_INVERTOARE_FOTO3": "",
  "IX_MAP_PI_INVERTOARE_FOTO4": "",
  "IX_MAP_PI_INVERTOARE_FOTO5": "",
  "IX_MAP_PI_INVERTOARE_FOTO_T": "4,000",
  "IX_MAP_CAPACITATE_STOCARE_AH1": "",
  "IX_MAP_CAPACITATE_STOCARE_AH2": "",
  "IX_MAP_CAPACITATE_STOCARE_AH3": "",
  "IX_MAP_CAPACITATE_STOCARE_AH4": "",
  "IX_MAP_CAPACITATE_STOCARE_AH5": "",
  "IX_MAP_CAPACITATE_STOCARE_AH_T": "0,00",
  "IX_MAP_PMAX_INVERTOARE_FOTO1": "4,000",
  "IX_MAP_PMAX_INVERTOARE_FOTO2": "0,000",
  "IX_MAP_PMAX_INVERTOARE_FOTO3": "0,000",
  "IX_MAP_PMAX_INVERTOARE_FOTO4": "0,000",
  "IX_MAP_PMAX_INVERTOARE_FOTO5": "0,000",
  "IX_MAP_PMAX_INVERTOARE_FOTO_T": "4,000",
  "IX_MAP_PMAX_CENTRAL_INVERTOARE_FOTO1": "",
  "IX_MAP_PMAX_CENTRAL_INVERTOARE_FOTO2": "",
  "IX_MAP_PMAX_CENTRAL_INVERTOARE_FOTO3": "",
  "IX_MAP_PMAX_CENTRAL_INVERTOARE_FOTO4": "",
  "IX_MAP_PMAX_CENTRAL_INVERTOARE_FOTO5": "",
  "IX_MAP_PMAX_CENTRAL_INVERTOARE_FOTO_T": "0,000",
  "IX_MAP_INVERTOARE_OBS1": "",
  "IX_MAP_INVERTOARE_OBS2": "",
  "IX_MAP_INVERTOARE_OBS3": "",
  "IX_MAP_INVERTOARE_OBS4": "",
  "IX_MAP_INVERTOARE_OBS5": "",
  "IX_MAP_HVDC_UN_CA1": "",
  "IX_MAP_HVDC_UN_CC1": "",
  "IX_MAP_HVDC_UN_CA_FAZA1": "",
  "IX_MAP_HVDC_PMAX_ABS1": "",
  "IX_MAP_HVDC_PMAX_EVAC1": "",
  "IX_MAP_HVDC_QMAX_ABS1": "",
  "IX_MAP_HVDC_QMAX_EVAC1": "",
  "IX_MAP_HVDC_OBS1": "",
  "IX_MAP_TENS_RAC_STOCARE": "",
  "IX_MAP_IS1_TIP_IS1": "",
  "IX_MAP_IS1_PI_IS1": "",
  "IX_MAP_IS1_PMAX_EVAC_IS1": "",
  "IX_MAP_IS1_PMAX_ABS_IS1": "",
  "IX_MAP_IS1_CAPACITATE_MAX_IS1": "",
  "IX_MAP_IS1_OBS1": "",
  "IX_MAP_IS2_NR_ELEM_STOCARE1": "",
  "IX_MAP_IS2_PI_ELEM_STOCARE1": "",
  "IX_MAP_IS2_CAPACITATE_MAX_ELEM_STOCARE1": "",
  "IX_MAP_IS2_QMAX_EVAC_INCARCARE1": "",
  "IX_MAP_IS2_QMAX_ABS_INCARCARE1": "",
  "IX_MAP_IS2_QMAX_EVAC_DESCARCARE1": "",
  "IX_MAP_IS2_QMAX_ABS_DESCARCARE1": "",
  "IX_MAP_IS2_OBS1": "",
  "IX_MAP_INJECTIE_PUTERE": "Cu injectie de putere in retea",
  "IX_GRP_TIP_SURSA_ENERGIE": "INSTALATIE DE STOCARE",
  "IX_MAP_PI_EV_AN_N": "",
  "IX_MAP_PI_EV_AN_N1": "",
  "IX_MAP_PI_EV_AN_N2": "",
  "IX_MAP_PI_EV_AN_N3": "",
  "IX_MAP_PI_EV_SF": "",
  "IX_MAP_PA_EV_AN_N": "",
  "IX_MAP_PA_EV_AN_N1": "",
  "IX_MAP_PA_EV_AN_N2": "",
  "IX_MAP_PA_EV_AN_N3": "",
  "IX_MAP_PA_EV_SF": "",
  "IX_MAP_PUTERE_PA_SOLICITARE_EV": "0,000",
  "IX_MAP_PUTERE_SA_SOLICITARE_EV": "",
  "IX_MAP_SA_EV_AN_N": "0,000",
  "IX_MAP_SA_EV_AN_N1": "0,000",
  "IX_MAP_SA_EV_AN_N2": "0,000",
  "IX_MAP_SA_EV_AN_N3": "0,000",
  "IX_MAP_SA_EV_SF": "0,000",
  "IX_GRP_PUTERE_SA_SOLICITARE_EV": "0",
  "IX_MAP_TENSIUNE_EV_EXISTENT": "",
  "IX_MAP_PUTERE_MAX_EVAC_E1_KWA": "0,000",
  "IX_MAP_PUTERE_MAX_EVAC_E2_KWA": "0,000",
  "IX_MAP_PUTERE_MAX_EVAC_E3_KWA": "0,000",
  "IX_MAP_PUTERE_MAX_EVAC_E4_KWA": "0,000",
  "IX_MAP_PUTERE_MAX_EVAC_EF_KWA": "0,000",
  "IX_MAP_PUTERE_MAX_EVAC_E1_KW": "0",
  "IX_MAP_PUTERE_MAX_EVAC_E2_KW": "0",
  "IX_MAP_PUTERE_MAX_EVAC_E3_KW": "0",
  "IX_MAP_PUTERE_MAX_EVAC_E4_KW": "0",
  "IX_MAP_PUTERE_MAX_EVAC_EF_KW": "0",
  "IX_MAP_PI_CONS_AN_N": "",
  "IX_MAP_PI_CONS_AN_N1": "",
  "IX_MAP_PI_CONS_AN_N2": "",
  "IX_MAP_PI_CONS_AN_N3": "",
  "IX_MAP_PI_CONS_SF": "",
  "IX_MAP_PA_CONS_AN_N": "",
  "IX_MAP_PA_CONS_AN_N1": "",
  "IX_MAP_PA_CONS_AN_N2": "",
  "IX_MAP_PA_CONS_AN_N3": "",
  "IX_MAP_PA_CONS_SF": "",
  "IX_GRP_PUTERE_SA_SOLICITARE_CONSUM": "0,000",
  "IX_MAP_SA_CONS_AN_N": "0,000",
  "IX_MAP_SA_CONS_AN_N1": "0,000",
  "IX_MAP_SA_CONS_AN_N2": "0,000",
  "IX_MAP_SA_CONS_AN_N3": "0,000",
  "IX_MAP_SA_CONS_SF": "0,000",
  "IX_MAP_PUTERE_MAX_ABSORBITA_EXIST_KVA": "0",
  "IX_MAP_PUTERE_MAX_ABSORBITA_EXIST_KW": "0",
  "IX_MAP_PUTERE_MAX_ABSORBITA_E1_KW": "0",
  "IX_MAP_PUTERE_MAX_ABSORBITA_E2_KW": "0",
  "IX_MAP_PUTERE_MAX_ABSORBITA_E3_KW": "0",
  "IX_MAP_PUTERE_MAX_ABSORBITA_E4_KW": "0",
  "IX_MAP_PUTERE_MAX_ABSORBITA_EF_KW": "0",
  "IX_MAP_PI_CENTRALA": "",
  "IX_MAP_PA_CENTRALA": "",
  "IX_GRP_COND_FUNCTIONARE_SPECIALE": "",
  "IX_GRP_DATE_PUNCT_PRODUCERE": "",
  "IX_GRP_DATE_PUNCT_DELIMITARE": "",
  "IX_GRP_ALTE_DATE_SPECIFICE": "",
  "IX_GRP_DATA_ESTIMATA_SOLICITARE": "",
  "IX_MAP_IMPLICATII": "",
  "IX_MAP_DECL_AUTENT": "Da",
  "IX_MAP_ACORD_FCT_ELEC": "Da",
  "IX_GRP_ADRESA_CORESP_SOLICITARE": "Adresa Obiectivului",
  "IX_MAP_JUDET_CORESPONDENTA": "Bihor",
  "IX_MAP_TIP_LOCALITATE_CORESPONDENTA": "",
  "IX_MAP_LOCALITATE_CORESPONDENTA": "CIUMEGHIU",
  "IX_MAP_TEMP_TIPCOR_LOCALITATE": "",
  "IX_MAP_SAT_CORESPONDENTA": "GHIORAC",
  "IX_MAP_TEMP_TIPCOR_STRADA": "",
  "IX_MAP_STRADA_CORESPONDENTA": "GHIORAC",
  "IX_MAP_NR_STRADA_CORESPONDENTA": "2",
  "IX_MAP_BLOC_CORESPONDENTA": "",
  "IX_MAP_SCARA_CORESPONDENTA": "",
  "IX_MAP_ETAJ_CORESPONDENTA": "",
  "IX_MAP_APARTAMENT_CORESPONDENTA": "",
  "IX_MAP_COD_POSTAL_CORESPONDENTA": "417197",
  "IX_MAP_EMAIL_CORESPONDENTA": "",
  "IX_MAP_NR_TELEFON_CORESPONDENTA": "",
  "IX_MAP_TARA_CORESPONDENTA": "",
  "IX_MAP_ALTE_OBSERVATII": "",
  "IX_MAP_REC1": "",
  "IX_MAP_PI_REC1": "",
  "IX_MAP_REC1_OBS": "",
  "IX_MAP_REC2": "",
  "IX_MAP_PI_REC2": "",
  "IX_MAP_REC2_OBS": "",
  "IX_MAP_REC3": "",
  "IX_MAP_PI_REC3": "",
  "IX_MAP_REC3_OBS": "",
  "IX_MAP_PI_REC_TOTAL": "0,000",
  "IX_MAP_MIJLOACE_COMPENSARE": "NU",
  "IX_MAP_TIP_COMP1": "",
  "IX_MAP_QN_COMP1": "",
  "IX_MAP_QMIN_COMP1": "",
  "IX_MAP_QMAX_COMP1": "",
  "IX_MAP_NR_TREPTE_COMP1": "",
  "IX_MAP_OBS_COMP1": "",
  "IX_MAP_TIP_COMP2": "",
  "IX_MAP_QN_COMP2": "",
  "IX_MAP_QMIN_COMP2": "",
  "IX_MAP_QMAX_COMP2": "",
  "IX_MAP_NR_TREPTE_COMP2": "",
  "IX_MAP_OBS_COMP2": "",
  "IX_MAP_SISTEME_STOCARE": "DETIN",
  "IX_MAP_CAP_BAT_ACUMULATOARE": "",
  "IX_MAP_ECHIPAMENTE_MASURA": "DA",
  "IX_MAP_SERIE_CONTOR1": "",
  "IX_MAP_TIP_CONTOR1": "",
  "IX_MAP_DT_CONTOR1": "",
  "IX_MAP_SERIE_CONTOR2": "",
  "IX_MAP_TIP_CONTOR2": "",
  "IX_MAP_DT_CONTOR2": "",
  "IX_MAP_PUNCT_RACORDARE": "",
  "IX_MAP_TENSIUNE_NOMINALA": "",
  "IX_MAP_PUTERE_NOMINALA": "",
  "IX_MAP_TENSIUNE_GENERATOARE": "",
  "IX_MAP_FRECVENTA_FUNCTIONARE": "",
  "IX_MAP_PUTERE_ACTIVA_MINIMA": "",
  "IX_MAP_PUTERE_REACTIVA_MAX": "",
  "IX_MAP_PUTERE_REACTIVA_MIN": "",
  "IX_MAP_CAPABILITATE_DE_TRECERE": "",
  "IX_MAP_NR_PANOURI_FOT": "",
  "IX_MAP_PUTERE_NOMINALA_PANOU": "",
  "IX_MAP_PUTERE_MAX_FOTOV": "",
  "IX_MAP_NR_INVERTOARE_ANEXA": "",
  "IX_MAP_TIP_INVERTOR": "",
  "IX_MAP_TIP_HIBRID": "",
  "IX_MAP_PUTERE_INTRARE": "",
  "IX_MAP_PUTEREA_MAX_INTR": "",
  "IX_MAP_DOMENIU_TENS_INTR": "",
  "IX_MAP_TENS_MAX_INTR": "",
  "IX_MAP_CURENT_MAXIM_INTR": "",
  "IX_MAP_PUTERE_ACT_NOM_IES": "",
  "IX_MAP_PUTERE_ACT_MAX_IES": "",
  "IX_MAP_PUTERE_REACT_NOM_IES": "",
  "IX_MAP_TENS_NOM_IES": "",
  "IX_MAP_CURENT_NOM_IES": "",
  "IX_MAP_DOMENIU_FRECV": "",
  "IX_MAP_NR_MAX_VAR": "",
  "IX_MAP_VAL_MAX_VAR_RAP_TEN": "",
  "IX_MAP_FACT_TOT_DIST": "",
  "IX_MAP_ARMONICE_CURENT_ELECT": "",
  "IX_MAP_FACT_TOT_DIS_TEN": "",
  "IX_MAP_ARMONICE_TENS": "",
  "IX_MAP_TIP_UNITATE_EOL": "",
  "IX_MAP_DIAMETRU": "",
  "IX_MAP_INALTIME_AX": "",
  "IX_MAP_SIS_COM_PALE": "",
  "IX_MAP_SIS_COM_VITEZA": "",
  "IX_MAP_TIP_GENERATOR_ANEXA": "",
  "IX_MAP_TIP_CONV_FR": "",
  "IX_MAP_VIT_VAR_PUT_ACT": "",
  "IX_MAP_CURENT_NOM": "",
  "IX_MAP_TENS_NOMINALA": "",
  "IX_MAP_VIT_VANT_PORNIRE": "",
  "IX_MAP_VIT_VANT_PUT_NOM": "",
  "IX_MAP_VIT_VANT_DEC": "",
  "IX_MAP_VIT_PUT_GENERATE": "",
  "IX_MAP_COEF_FLICKER": "",
  "IX_MAP_FACT_TREAPTA_FLICK": "",
  "IX_MAP_FACT_VAR_TENSIUNE": "",
  "IX_MAP_NR_OPER_COM": "",
  "IX_MAP_PUT_ACT_MAX_PROD": "",
  "IX_MAP_PUT_ACT_MIN_PROD": "",
  "IX_MAP_PUT_REACT_MAX_BOR": "",
  "IX_MAP_PUT_REACT_MIN_BOR": "",
  "IX_MAP_TENSI_NOMINALA": "",
  "IX_MAP_FRECV_MAX_MIN": "",
  "IX_MAP_REACTANTA_NOM": "",
  "IX_MAP_REACT_SECV": "",
  "IX_MAP_REAC_SECV_NEG": "",
  "IX_MAP_PUTERE_ABS": "",
  "IX_MAP_T_NOMINALA": "",
  "IX_MAP_PST_SOLICITANT": "ADSA DASDAS",
  "IX_MAP_NECESITA_CERTIFICAT_CONFORMITATE": "NU",
  "NEXTNODE": "116",
  "force": True,
  "ELO_REMOVED_ELEMENTS": []
}


    resp = session.post(
    url,
    params=params,
    headers=headers,
    json=payload,
    timeout=30,
    )

    print(resp.status_code)



if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5001, debug=True)