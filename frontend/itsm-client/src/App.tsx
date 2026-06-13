import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import './App.css'

type AuthMode = 'login' | 'register'

type UserDto = {
  id: number
  userName: string
  email: string
  fullName?: string
  isActive: boolean
  roles: string[]
}

type AuthResponse = {
  success: boolean
  message: string
  token?: string
  user?: UserDto
}

type LoginForm = {
  userName: string
  password: string
}

type RegisterForm = LoginForm & {
  email: string
  fullName: string
}

const initialLoginForm: LoginForm = {
  userName: '',
  password: '',
}

const initialRegisterForm: RegisterForm = {
  userName: '',
  email: '',
  fullName: '',
  password: '',
}

function App() {
  const [mode, setMode] = useState<AuthMode>('login')
  const [loginForm, setLoginForm] = useState<LoginForm>(initialLoginForm)
  const [registerForm, setRegisterForm] = useState<RegisterForm>(initialRegisterForm)
  const [token, setToken] = useState(() => localStorage.getItem('itsm.token') ?? '')
  const [user, setUser] = useState<UserDto | null>(() => {
    const storedUser = localStorage.getItem('itsm.user')
    if (!storedUser) {
      return null
    }

    const parsedUser = JSON.parse(storedUser) as UserDto
    return {
      ...parsedUser,
      roles: parsedUser.roles ?? [],
    }
  })
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)

  const activeForm = mode === 'login' ? loginForm : registerForm
  const canSubmit = useMemo(() => {
    if (mode === 'login') {
      return loginForm.userName.trim().length > 0 && loginForm.password.length > 0
    }

    return (
      registerForm.userName.trim().length >= 3 &&
      registerForm.email.trim().length > 0 &&
      registerForm.fullName.trim().length >= 2 &&
      registerForm.password.length >= 6
    )
  }, [loginForm, mode, registerForm])

  useEffect(() => {
    if (token) {
      localStorage.setItem('itsm.token', token)
    } else {
      localStorage.removeItem('itsm.token')
    }
  }, [token])

  useEffect(() => {
    if (user) {
      localStorage.setItem('itsm.user', JSON.stringify(user))
    } else {
      localStorage.removeItem('itsm.user')
    }
  }, [user])

  async function submitAuth(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError('')
    setMessage('')
    setIsLoading(true)

    const endpoint = mode === 'login' ? '/api/auth/login' : '/api/auth/register'
    const body = mode === 'login' ? loginForm : registerForm

    try {
      const response = await fetch(endpoint, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(body),
      })

      const data = (await response.json()) as AuthResponse

      if (!response.ok || !data.success) {
        setError(data.message || 'Pedido recusado pela API.')
        return
      }

      setUser(data.user ?? null)
      setMessage(data.message)

      if (data.token) {
        setToken(data.token)
        setLoginForm(initialLoginForm)
      }

      if (mode === 'register') {
        setRegisterForm(initialRegisterForm)
        setLoginForm({ userName: registerForm.userName, password: '' })
        setMode('login')
      }
    } catch {
      setError('Nao foi possivel contactar a API. Confirma que o backend esta a correr.')
    } finally {
      setIsLoading(false)
    }
  }

  async function loadCurrentUser() {
    if (!token) {
      setError('Faz login para testar o endpoint protegido.')
      return
    }

    setError('')
    setMessage('')
    setIsLoading(true)

    try {
      const response = await fetch('/api/auth/me', {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      })

      if (!response.ok) {
        setError('Token recusado pela API.')
        return
      }

      const currentUser = (await response.json()) as UserDto
      setUser(currentUser)
      setMessage('Ligacao validada com /api/auth/me.')
    } catch {
      setError('Nao foi possivel contactar a API.')
    } finally {
      setIsLoading(false)
    }
  }

  function logout() {
    setToken('')
    setUser(null)
    setMode('login')
    setMessage('Sessao terminada no browser.')
    setError('')
  }

  if (user && token) {
    return (
      <HomePage
        isLoading={isLoading}
        message={message}
        onLogout={logout}
        onRefreshUser={loadCurrentUser}
        user={user}
      />
    )
  }

  return (
    <main className="app-shell">
      <section className="auth-panel" aria-labelledby="auth-title">
        <div className="panel-heading">
          <p className="eyebrow">ITSM System</p>
          <h1 id="auth-title">Autenticacao</h1>
          <p className="lead">Cria uma conta ou inicia sessao para validar a ligacao ao ASP.NET Core Identity.</p>
        </div>

        <div className="mode-switch" role="tablist" aria-label="Modo de autenticacao">
          <button
            type="button"
            className={mode === 'login' ? 'active' : ''}
            onClick={() => setMode('login')}
          >
            Login
          </button>
          <button
            type="button"
            className={mode === 'register' ? 'active' : ''}
            onClick={() => setMode('register')}
          >
            Cadastro
          </button>
        </div>

        <form className="auth-form" onSubmit={submitAuth}>
          {mode === 'register' && (
            <>
              <label>
                Nome completo
                <input
                  autoComplete="name"
                  minLength={2}
                  name="fullName"
                  required
                  type="text"
                  value={registerForm.fullName}
                  onChange={(event) =>
                    setRegisterForm((form) => ({ ...form, fullName: event.target.value }))
                  }
                />
              </label>

              <label>
                Email
                <input
                  autoComplete="email"
                  name="email"
                  required
                  type="email"
                  value={registerForm.email}
                  onChange={(event) =>
                    setRegisterForm((form) => ({ ...form, email: event.target.value }))
                  }
                />
              </label>
            </>
          )}

          <label>
            Utilizador
            <input
              autoComplete="username"
              minLength={mode === 'register' ? 3 : undefined}
              name="userName"
              required
              type="text"
              value={activeForm.userName}
              onChange={(event) => {
                if (mode === 'login') {
                  setLoginForm((form) => ({ ...form, userName: event.target.value }))
                  return
                }

                setRegisterForm((form) => ({ ...form, userName: event.target.value }))
              }}
            />
          </label>

          <label>
            Palavra-passe
            <input
              autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
              minLength={6}
              name="password"
              required
              type="password"
              value={activeForm.password}
              onChange={(event) => {
                if (mode === 'login') {
                  setLoginForm((form) => ({ ...form, password: event.target.value }))
                  return
                }

                setRegisterForm((form) => ({ ...form, password: event.target.value }))
              }}
            />
          </label>

          <button className="primary-action" disabled={!canSubmit || isLoading} type="submit">
            {isLoading ? 'A processar...' : mode === 'login' ? 'Entrar' : 'Criar conta'}
          </button>
        </form>

        {(message || error) && (
          <p className={error ? 'feedback error' : 'feedback success'}>{error || message}</p>
        )}
      </section>

      <aside className="session-panel" aria-label="Estado da sessao">
        <div>
          <p className="eyebrow">Estado</p>
          <h2>{user ? user.fullName || user.userName : 'Sem sessao ativa'}</h2>
          <p className="session-copy">
            {user
              ? 'Os dados abaixo vieram da API e podem ser confirmados nas tabelas AspNetUsers.'
              : 'Depois do cadastro, usa o login para receber um token JWT.'}
          </p>
        </div>

        {user && (
          <dl className="user-details">
            <div>
              <dt>ID</dt>
              <dd>{user.id}</dd>
            </div>
            <div>
              <dt>Username</dt>
              <dd>{user.userName}</dd>
            </div>
            <div>
              <dt>Email</dt>
              <dd>{user.email}</dd>
            </div>
            <div>
              <dt>Ativo</dt>
              <dd>{user.isActive ? 'Sim' : 'Nao'}</dd>
            </div>
            <div>
              <dt>Roles</dt>
              <dd>{user.roles.length > 0 ? user.roles.join(', ') : 'Sem role'}</dd>
            </div>
          </dl>
        )}

        <div className="session-actions">
          <button type="button" onClick={loadCurrentUser} disabled={!token || isLoading}>
            Testar token
          </button>
          <button type="button" onClick={logout} disabled={!token && !user}>
            Sair
          </button>
        </div>
      </aside>
    </main>
  )
}

type HomePageProps = {
  isLoading: boolean
  message: string
  onLogout: () => void
  onRefreshUser: () => void
  user: UserDto
}

function HomePage({ isLoading, message, onLogout, onRefreshUser, user }: HomePageProps) {
  return (
    <main className="home-shell">
      <header className="topbar">
        <div>
          <p className="eyebrow">ITSM System</p>
          <h1>Painel inicial</h1>
        </div>

        <div className="topbar-actions">
          <button type="button" onClick={onRefreshUser} disabled={isLoading}>
            Atualizar sessao
          </button>
          <button type="button" onClick={onLogout}>
            Sair
          </button>
        </div>
      </header>

      <section className="welcome-band">
        <div>
          <p className="eyebrow">Bem-vindo</p>
          <h2>{user.fullName || user.userName}</h2>
          <p>Conta ativa com perfil {user.roles.length > 0 ? user.roles.join(', ') : 'sem role'}.</p>
        </div>

        <dl className="account-summary">
          <div>
            <dt>ID</dt>
            <dd>{user.id}</dd>
          </div>
          <div>
            <dt>Email</dt>
            <dd>{user.email}</dd>
          </div>
          <div>
            <dt>Estado</dt>
            <dd>{user.isActive ? 'Ativo' : 'Inativo'}</dd>
          </div>
        </dl>
      </section>

      <section className="home-grid" aria-label="Resumo operacional">
        <article>
          <span className="metric">0</span>
          <h3>Tickets abertos</h3>
          <p>A fase de tickets entra no proximo passo do projeto.</p>
        </article>
        <article>
          <span className="metric">0</span>
          <h3>Tickets atribuidos</h3>
          <p>Quando houver tecnicos, esta area mostra o trabalho em curso.</p>
        </article>
        <article>
          <span className="metric">{user.roles.length}</span>
          <h3>Roles</h3>
          <p>{user.roles.length > 0 ? user.roles.join(', ') : 'Sem role atribuida'}</p>
        </article>
      </section>

      {message && <p className="home-feedback">{message}</p>}
    </main>
  )
}

export default App
