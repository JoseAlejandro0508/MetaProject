import telebot
import requests
import os
import uuid
from pathlib import Path

# ── Configuration from environment variables ─────────────────────────
TOKEN = os.environ.get("TELEGRAM_BOT_TOKEN", "")
ADMIN_IDS = os.environ.get("ADMIN_IDS", "").split(",")
API_BASE_URL = os.environ.get("API_BASE_URL", "http://localhost:5071")
API_KEY = os.environ.get("ADMIN_API_KEY", "your-secret-api-key-here")

# API Endpoints
API_CONFIRM_DEPOSIT = f"{API_BASE_URL}/api/Wallet/AdminConfirmTransaction"
API_UPDATE_BALANCE = f"{API_BASE_URL}/api/Wallet/AdminUpdateBalance"
API_GET_BALANCE = f"{API_BASE_URL}/api/Wallet/GetBalance/"
API_GET_USER_INFO = f"{API_BASE_URL}/api/User/AdminGetUserInfo/"
API_WITHDRAW = f"{API_BASE_URL}/api/Wallet/WithdrawBalance"
API_GET_VR = f"{API_BASE_URL}/api/UserPlans/GetVrsToUser/"
API_POST_VR = f"{API_BASE_URL}/api/UserPlans/AdminPlanToUser"
API_DELETE_VR = f"{API_BASE_URL}/api/UserPlans/DeleteUserPlans"

bot = telebot.TeleBot(TOKEN)

# Per-chat state management (keyed by chat.id)
chat_states = {}


def get_state(chat_id):
    if chat_id not in chat_states:
        chat_states[chat_id] = {"state": "IDLE", "data": {}}
    return chat_states[chat_id]


def set_state(chat_id, state, data=None):
    chat_states[chat_id] = {"state": state, "data": data or {}}


def clear_state(chat_id):
    chat_states[chat_id] = {"state": "IDLE", "data": {}}


def is_admin(user_id):
    return True


def api_headers():
    return {"X-Api-Key": API_KEY, "Content-Type": "application/json"}


# ── /cancel command ────────────────────────────────────────────────────
@bot.message_handler(commands=["cancel"])
def cancel_command(message):
    clear_state(message.chat.id)
    bot.reply_to(message, "✅ Operación cancelada.")


# ── /recarga command ───────────────────────────────────────────────────
@bot.message_handler(commands=["recarga"])
def iniciar_recarga(message):
    if not is_admin(message.from_user.id):
        bot.reply_to(message, "❌ No tienes permisos para realizar esta acción.")
        return

    bot.reply_to(
        message, "⚠️ Por favor envíe el número del usuario al que desea agregarle saldo:"
    )
    set_state(message.chat.id, "WAITING_PHONE_RECARGA")


@bot.message_handler(commands=["confirmWithdraw"])
def confirmWithdraw(message):
    if not is_admin(message.from_user.id):
        bot.reply_to(message, "❌ No tienes permisos para realizar esta acción.")
        return

    bot.reply_to(
        message,
        "Ingrese el numero del usuario",
    )
    set_state(message.chat.id, "WAITING_PHONE_CONFIRM_W")


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_PHONE_CONFIRM_W"
)
def recibir_phone_w(message):
    state = get_state(message.chat.id)
    state["data"]["phone"] = message.text.strip()
    bot.reply_to(
        message,
        "⬇️ Por favor envíe la orden de retiro a confirmar",
    )
    set_state(message.chat.id, "WAITING_ID_W", state["data"])


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_ID_W"
)
def recibir_id_w(message):
    try:
        id = message.text

        state = get_state(message.chat.id)
        phone = state["data"]["phone"]

        payload = {"PhoneOrEmail": phone, "OrdenId": id, "Type": "WITHDRAW"}
        response = requests.post(
            API_CONFIRM_DEPOSIT, json=payload, headers=api_headers()
        )

        if response.status_code == 200:
            data = response.json()
            new_balance = data.get("newBalance", "N/A")
            bot.reply_to(message, f"✅️ Listo el balance del usuario {new_balance}")
        else:
            error_msg = response.json().get("message", "Error desconocido")
            bot.reply_to(message, f"❌ Error: {error_msg}")
    except ValueError:
        bot.reply_to(message, "❌ Por favor ingrese orden valida")
    except Exception as e:
        bot.reply_to(message, f"❌ Error: {str(e)}")
    finally:
        clear_state(message.chat.id)





@bot.message_handler(commands=["confirmDeposit"])
def confirmDeposit(message):
    if not is_admin(message.from_user.id):
        bot.reply_to(message, "❌ No tienes permisos para realizar esta acción.")
        return

    bot.reply_to(
        message,
        "Ingrese el numero del usuario",
    )
    set_state(message.chat.id, "WAITING_PHONE_CONFIRM")


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_PHONE_CONFIRM"
)
def recibir_phone_confirm(message):
    state = get_state(message.chat.id)
    state["data"]["phone"] = message.text.strip()
    bot.reply_to(
        message,
        "⬇️ Por favor envíe la orden de deposito a confirmar",
    )
    set_state(message.chat.id, "WAITING_ID_DEPOSIT", state["data"])


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_ID_DEPOSIT"
)
def recibir_id_deposit(message):
    try:
        id = message.text

        state = get_state(message.chat.id)
        phone = state["data"]["phone"]

        payload = {"PhoneOrEmail": phone, "OrdenId": id, "Type": "DEPOSIT"}
        response = requests.post(
            API_CONFIRM_DEPOSIT, json=payload, headers=api_headers()
        )

        if response.status_code == 200:
            data = response.json()
            new_balance = data.get("newBalance", "N/A")
            bot.reply_to(message, f"✅️ Listo el balance del usuario {new_balance}")
        else:
            error_msg = response.json().get("message", "Error desconocido")
            bot.reply_to(message, f"❌ Error: {error_msg}")
    except ValueError:
        bot.reply_to(message, "❌ Por favor ingrese orden valida")
    except Exception as e:
        bot.reply_to(message, f"❌ Error: {str(e)}")
    finally:
        clear_state(message.chat.id)


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_PHONE_RECARGA"
)
def recibir_phone_recarga(message):
    state = get_state(message.chat.id)
    state["data"]["phone"] = message.text.strip()
    bot.reply_to(
        message,
        "⬇️ Por favor envíe la cantidad de saldo que deseas agregar a la cuenta del usuario:",
    )
    set_state(message.chat.id, "WAITING_AMOUNT_RECARGA", state["data"])


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_AMOUNT_RECARGA"
)
def recibir_amount_recarga(message):
    try:
        amount = float(message.text.strip())
        if amount <= 0:
            raise ValueError("Monto debe ser positivo")

        state = get_state(message.chat.id)
        phone = state["data"]["phone"]

        payload = {"PhoneOrEmail": phone, "Amount": amount}
        response = requests.post(
            API_UPDATE_BALANCE, json=payload, headers=api_headers()
        )

        if response.status_code == 200:
            data = response.json()
            new_balance = data.get("newBalance", "N/A")
            bot.reply_to(message, f"✅️ Listo el balance del usuario {new_balance}")
        else:
            error_msg = response.json().get("message", "Error desconocido")
            bot.reply_to(message, f"❌ Error: {error_msg}")
    except ValueError:
        bot.reply_to(message, "❌ Por favor ingrese un monto válido (número positivo).")
    except Exception as e:
        bot.reply_to(message, f"❌ Error: {str(e)}")
    finally:
        clear_state(message.chat.id)


# ── /retirada command ──────────────────────────────────────────────────
@bot.message_handler(commands=["retirada"])
def iniciar_retirada(message):
    if not is_admin(message.from_user.id):
        bot.reply_to(message, "❌ No tienes permisos para realizar esta acción.")
        return

    bot.reply_to(
        message, "⚠️ Por favor envíe el número del usuario al que desea quitarle saldo:"
    )
    set_state(message.chat.id, "WAITING_PHONE_RETIRADA")


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_PHONE_RETIRADA"
)
def recibir_phone_retirada(message):
    state = get_state(message.chat.id)
    state["data"]["phone"] = message.text.strip()
    bot.reply_to(
        message,
        "⬆️ Por favor envíe la cantidad de saldo que deseas quitarle a la cuenta del usuario:",
    )
    set_state(message.chat.id, "WAITING_AMOUNT_RETIRADA", state["data"])


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_AMOUNT_RETIRADA"
)
def recibir_amount_retirada(message):
    try:
        amount = float(message.text.strip())
        if amount <= 0:
            raise ValueError("Monto debe ser positivo")

        state = get_state(message.chat.id)
        phone = state["data"]["phone"]

        payload = {"PhoneOrEmail": phone, "Amount": -amount}
        response = requests.post(
            API_UPDATE_BALANCE, json=payload, headers=api_headers()
        )

        if response.status_code == 200:
            data = response.json()
            new_balance = data.get("newBalance", "N/A")
            bot.reply_to(message, f"✅️ Listo el balance del usuario es {new_balance}")
        else:
            error_msg = response.json().get("message", "Error desconocido")
            bot.reply_to(message, f"❌ Error: {error_msg}")
    except ValueError:
        bot.reply_to(message, "❌ Por favor ingrese un monto válido (número positivo).")
    except Exception as e:
        bot.reply_to(message, f"❌ Error: {str(e)}")
    finally:
        clear_state(message.chat.id)


# ── /info command ──────────────────────────────────────────────────────
@bot.message_handler(commands=["info"])
def iniciar_info(message):
    if not is_admin(message.from_user.id):
        bot.reply_to(message, "❌ No tienes permisos para realizar esta acción.")
        return

    bot.reply_to(
        message, "⚠️ Por favor envíe el número del usuario para mostrar su información:"
    )
    set_state(message.chat.id, "WAITING_PHONE_INFO")


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_PHONE_INFO"
)
def recibir_phone_info(message):
    try:
        phone = message.text.strip()

        response = requests.get(f"{API_GET_USER_INFO}{phone}", headers=api_headers())

        if response.status_code == 200:
            data = response.json()

            info_text = f"""✅️ Información del usuario **{phone}**

● Referidos: {data.get('referralsLevel1', 0)}
● Balance: {data.get('balance', 0):,.0f}
● Dinero Depositado: {data.get('totalDeposited', 0):,.0f}
● Dinero Retirado: {data.get('totalWithdrawn', 0):,.0f}
● Fecha en la que se unió: {data.get('joinDate', 'N/A')}
● Inversiones activas:"""

            investments = data.get("activeInvestments", [])
            if investments:
                for inv in investments:
                    info_text += f"\n  - ID: {inv.get('id', 'N/A')}, Beneficio acumulado: {inv.get('accumulatedBenefit', 0):,.0f}"
            else:
                info_text += "\n  Ninguna"

            bot.reply_to(message, info_text, parse_mode="Markdown")
        else:
            error_msg = response.json().get("message", "Usuario no encontrado")
            bot.reply_to(message, f"❌ Error: {error_msg}")
    except Exception as e:
        bot.reply_to(message, f"❌ Error: {str(e)}")
    finally:
        clear_state(message.chat.id)


# ── Existing commands (mantained) ──────────────────────────────────────


@bot.message_handler(commands=["actualizarsaldo"])
def iniciar_actualizar_saldo(message):
    if not is_admin(message.from_user.id):
        bot.reply_to(message, "❌ No tienes permisos para realizar esta acción.")
        return

    bot.reply_to(
        message,
        "Por favor, envía el usuario que desea actualizar su saldo (# de teléfono o correo).",
    )
    set_state(message.chat.id, "WAITING_USER_ACTUALIZAR")


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_USER_ACTUALIZAR"
)
def recibir_usuario_actualizar(message):
    state = get_state(message.chat.id)
    state["data"]["user_id"] = message.text.strip()
    bot.reply_to(message, "Ahora, envía el monto de la recarga.")
    set_state(message.chat.id, "WAITING_MONTO_ACTUALIZAR", state["data"])


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_MONTO_ACTUALIZAR"
)
def recibir_monto_actualizar(message):
    try:
        monto = float(message.text)
        state = get_state(message.chat.id)
        state["data"]["monto"] = monto
        bot.reply_to(
            message, "Finalmente, envía la moneda (ej. Nequi, TRX, USDT_TRC20, PayPal)."
        )
        set_state(message.chat.id, "WAITING_MONEDA_ACTUALIZAR", state["data"])
    except ValueError:
        bot.reply_to(message, "Por favor, ingrese un monto válido.")


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_MONEDA_ACTUALIZAR"
)
def recibir_moneda_actualizar(message):
    state = get_state(message.chat.id)
    payload = {
        "Email": state["data"]["user_id"],
        "Balance": state["data"]["monto"],
        "Token": message.text.strip(),
    }
    response = requests.post(
        f"{API_BASE_URL}/api/Wallet/UpdateBalance", json=payload, headers=api_headers()
    )

    if response.status_code == 200:
        bot.reply_to(message, "✅ Saldo actualizado con éxito.")
    else:
        bot.reply_to(message, f"❌ Error al actualizar el saldo: {response.text}")
    clear_state(message.chat.id)


@bot.message_handler(commands=["retirarsaldo"])
def iniciar_retiro_saldo(message):
    if not is_admin(message.from_user.id):
        bot.reply_to(
            message,
            "❌ Usted no es el administrador del bot, no puedes usar este comando.",
        )
        return

    bot.reply_to(
        message,
        "Por favor, envía el usuario que desea actualizar su saldo (# de teléfono o correo).",
    )
    set_state(message.chat.id, "WAITING_USER_RETIRO")


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_USER_RETIRO"
)
def recibir_usuario_retiro(message):
    state = get_state(message.chat.id)
    state["data"]["user_identifier"] = message.text.strip()
    bot.reply_to(message, "Ahora por favor envie el monto a retirar")
    set_state(message.chat.id, "WAITING_MONTO_RETIRO", state["data"])


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_MONTO_RETIRO"
)
def recibir_monto_retiro(message):
    try:
        monto = float(message.text)
        if monto <= 0:
            raise ValueError("El monto a retirar debe ser mayor a cero")
        state = get_state(message.chat.id)
        state["data"]["monto_a_retirar"] = monto

        payload = {
            "username": state["data"]["user_identifier"],
            "withdraw": state["data"]["monto_a_retirar"],
        }
        response = requests.patch(API_WITHDRAW, json=payload, headers=api_headers())
        response_data = response.json()
        if response.status_code == 200:
            bot.reply_to(
                message, response_data.get("message", "Saldo retirado con éxito")
            )
        else:
            bot.reply_to(
                message, response_data.get("message", "Error al retirar el saldo")
            )
    except ValueError:
        bot.reply_to(message, "Por favor entre un monto válido")
    except Exception as e:
        bot.reply_to(message, "Error al retirar el saldo")
    finally:
        clear_state(message.chat.id)


@bot.message_handler(commands=["obtenerbalance"])
def iniciar_obtener_balance(message):
    if not is_admin(message.from_user.id):
        bot.reply_to(
            message,
            "❌ Usted no es el administrador del bot, no puedes usar este comando.",
        )
        return

    bot.reply_to(
        message,
        "Por favor, envía el usuario que desea obtener su balance (# de teléfono o correo).",
    )
    set_state(message.chat.id, "WAITING_USER_BALANCE")


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_USER_BALANCE"
)
def recibir_usuario_balance(message):
    try:
        user = message.text.strip()
        response = requests.get(f"{API_GET_BALANCE}{user}", headers=api_headers())
        if response.status_code == 200:
            balance = response.json()
            bot.reply_to(message, f"💰 El saldo de ese usuario es {balance}")
        else:
            response_data = response.json()
            bot.reply_to(
                message, response_data.get("message", "Error al obtener el saldo")
            )
    except Exception as e:
        bot.reply_to(message, "Error al obtener el saldo")
    finally:
        clear_state(message.chat.id)


@bot.message_handler(commands=["obtenergafasvr"])
def iniciar_obtener_gafas(message):
    if not is_admin(message.from_user.id):
        bot.reply_to(
            message,
            "❌ Usted no es el administrador del bot, no puedes usar este comando.",
        )
        return

    bot.reply_to(
        message,
        "Por favor, envía el usuario que desea obtener sus gafas (# de teléfono o correo).",
    )
    set_state(message.chat.id, "WAITING_USER_GAFAS")


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_USER_GAFAS"
)
def recibir_usuario_gafas(message):
    try:
        user = message.text.strip()
        response = requests.get(f"{API_GET_VR}{user}", headers=api_headers())
        if response.status_code == 200:
            gafas = response.json()
            bot.reply_to(message, f"🥽 Las gafas de ese usuario son {gafas}")
        else:
            response_data = response.json()
            bot.reply_to(
                message, response_data.get("message", "Error al obtener las gafas")
            )
    except Exception as e:
        bot.reply_to(message, "Error al obtener las gafas")
    finally:
        clear_state(message.chat.id)


@bot.message_handler(commands=["comprargafavr"])
def iniciar_comprar_gafas(message):
    if not is_admin(message.from_user.id):
        bot.reply_to(
            message,
            "❌ Usted no es el administrador del bot, no puedes usar este comando.",
        )
        return

    bot.reply_to(
        message,
        "Por favor, envía el usuario que desea comprar gafas (# de teléfono o correo).",
    )
    set_state(message.chat.id, "WAITING_USER_COMPRAR_GAFAS")


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_USER_COMPRAR_GAFAS"
)
def recibir_usuario_comprar_gafas(message):
    state = get_state(message.chat.id)
    state["data"]["user_identifier"] = message.text.strip()
    bot.reply_to(
        message, "Ahora envie el id de la VR, por ejemplo 1 es VR1, 2 es VR2 etc"
    )
    set_state(message.chat.id, "WAITING_ID_COMPRAR_GAFAS", state["data"])


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_ID_COMPRAR_GAFAS"
)
def recibir_id_comprar_gafas(message):
    try:
        state = get_state(message.chat.id)
        state["data"]["id"] = int(message.text)
        payload = {
            "username": state["data"]["user_identifier"],
            "vr": state["data"]["id"],
        }
        response = requests.post(API_POST_VR, json=payload, headers=api_headers())
        if response.status_code == 200:
            data = response.json()
            bot.reply_to(message, data.get("message", "Gafa comprada con éxito"))
        else:
            response_data = response.json()
            bot.reply_to(
                message, response_data.get("message", "Error al comprar la gafa")
            )
    except Exception as e:
        bot.reply_to(message, "Error al comprar la gafa")
    finally:
        clear_state(message.chat.id)


@bot.message_handler(commands=["borrargafavr"])
def iniciar_borrar_gafas(message):
    if not is_admin(message.from_user.id):
        bot.reply_to(
            message,
            "❌ Usted no es el administrador del bot, no puedes usar este comando.",
        )
        return

    bot.reply_to(
        message,
        "Por favor, envía el usuario que desea borrar sus gafas (# de teléfono o correo).",
    )
    set_state(message.chat.id, "WAITING_USER_BORRAR_GAFAS")


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_USER_BORRAR_GAFAS"
)
def recibir_usuario_borrar_gafas(message):
    state = get_state(message.chat.id)
    state["data"]["user_identifier"] = message.text.strip()
    bot.reply_to(
        message, "Ahora envie el id de la VR, por ejemplo 1 es VR1, 2 es VR2 etc"
    )
    set_state(message.chat.id, "WAITING_ID_BORRAR_GAFAS", state["data"])


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_ID_BORRAR_GAFAS"
)
def recibir_id_borrar_gafas(message):
    try:
        state = get_state(message.chat.id)
        state["data"]["id"] = int(message.text)
        bot.reply_to(message, "Por favor ingrese la cantidad de gafas que desea borrar")
        set_state(message.chat.id, "WAITING_CANTIDAD_BORRAR_GAFAS", state["data"])
    except Exception as e:
        bot.reply_to(message, "Error: ID debe ser un número")
        clear_state(message.chat.id)


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_CANTIDAD_BORRAR_GAFAS"
)
def recibir_cantidad_borrar_gafas(message):
    try:
        state = get_state(message.chat.id)
        state["data"]["cantidad"] = int(message.text)
        payload = {
            "username": state["data"]["user_identifier"],
            "vr": state["data"]["id"],
            "quantity": state["data"]["cantidad"],
        }
        response = requests.delete(API_DELETE_VR, json=payload, headers=api_headers())
        if response.status_code == 200:
            data = response.json()
            bot.reply_to(message, data.get("message", "Gafa borrada con éxito"))
        else:
            response_data = response.json()
            bot.reply_to(
                message, response_data.get("message", "Error al borrar la gafa")
            )
    except Exception as e:
        bot.reply_to(message, "Error al borrar la gafa")
    finally:
        clear_state(message.chat.id)


# ── Image commands (mantained) ────────────────────────────────────────
directory = os.path.abspath("./imagenes")
channel_id = -1002226124389

if not os.path.exists(directory):
    os.makedirs(directory)


@bot.message_handler(commands=["listarimagenes"])
def listar_imagenes(message):
    image_files = (
        list(Path(directory).glob("*.jpg"))
        + list(Path(directory).glob("*.png"))
        + list(Path(directory).glob("*.webp"))
    )
    if not image_files:
        bot.reply_to(message, "No hay imágenes guardadas.")
    else:
        for img in image_files:
            with open(img, "rb") as image_file:
                bot.send_photo(
                    message.chat.id, image_file, caption=f"Imagen ID: {img.stem}"
                )


@bot.message_handler(commands=["eliminarimagenes"])
def iniciar_eliminar_imagen(message):
    bot.reply_to(message, "Por favor, envía el ID de la imagen que deseas eliminar.")
    set_state(message.chat.id, "WAITING_DELETE_IMAGE")


@bot.message_handler(
    func=lambda m: get_state(m.chat.id)["state"] == "WAITING_DELETE_IMAGE"
)
def eliminar_imagen(message):
    image_id = message.text.strip()
    image_path = Path(directory) / f"{image_id}.jpg"
    if image_path.exists():
        os.remove(image_path)
        bot.reply_to(message, f"Imagen con ID {image_id} eliminada exitosamente.")
    else:
        bot.reply_to(message, "No se encontró ninguna imagen con ese ID.")
    clear_state(message.chat.id)


@bot.message_handler(content_types=["photo"])
def guardar_imagen(message):
    try:
        file_info = bot.get_file(message.photo[-1].file_id)
        downloaded_file = bot.download_file(file_info.file_path)
        image_id = str(uuid.uuid4())
        image_extension = file_info.file_path.split(".")[-1]
        image_path = os.path.join(directory, f"{image_id}.{image_extension}")
        os.makedirs(directory, exist_ok=True)
        with open(image_path, "wb") as f:
            f.write(downloaded_file)
        bot.reply_to(message, f"Imagen guardada en {image_path}")
    except Exception as e:
        bot.reply_to(message, f"Error al guardar la imagen: {e}")


# ── Start bot ─────────────────────────────────────────────────────────
try:
    print("Bot está escuchando...")
    bot.polling(none_stop=True, interval=0)
except Exception as e:
    print(f"Error en el bot: {e}")
