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

type ApiMessage = {
  message?: string
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
        token={token}
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
  token: string
  user: UserDto
}

function HomePage({ isLoading, message, onLogout, onRefreshUser, token, user }: HomePageProps) {
  const isAdministrator = user.roles.includes('Administrador')

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

      {isAdministrator && <AdminRoleManagement currentUserId={user.id} token={token} />}

      {message && <p className="home-feedback">{message}</p>}
    </main>
  )
}

type AdminRoleManagementProps = {
  currentUserId: number
  token: string
}

function AdminRoleManagement({ currentUserId, token }: AdminRoleManagementProps) {
  const [availableRoles, setAvailableRoles] = useState<string[]>([])
  const [users, setUsers] = useState<UserDto[]>([])
  const [draftRoles, setDraftRoles] = useState<Record<number, string[]>>({})
  const [isLoading, setIsLoading] = useState(false)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  useEffect(() => {
    void loadRoleManagement()
  }, [])

  async function loadRoleManagement() {
    setIsLoading(true)
    setError('')
    setMessage('')

    try {
      const [rolesResponse, usersResponse] = await Promise.all([
        fetch('/api/admin/roles', {
          headers: { Authorization: `Bearer ${token}` },
        }),
        fetch('/api/admin/users', {
          headers: { Authorization: `Bearer ${token}` },
        }),
      ])

      if (!rolesResponse.ok || !usersResponse.ok) {
        setError('Nao foi possivel carregar a gestao de utilizadores.')
        return
      }

      const roles = (await rolesResponse.json()) as string[]
      const loadedUsers = (await usersResponse.json()) as UserDto[]

      setAvailableRoles(roles)
      setUsers(loadedUsers)
      setDraftRoles(
        Object.fromEntries(
          loadedUsers.map((loadedUser) => [loadedUser.id, loadedUser.roles ?? []]),
        ),
      )
    } catch {
      setError('Nao foi possivel contactar a API de administracao.')
    } finally {
      setIsLoading(false)
    }
  }

  function toggleRole(userId: number, role: string) {
    setDraftRoles((current) => {
      const roles = current[userId] ?? []
      const nextRoles = roles.includes(role)
        ? roles.filter((item) => item !== role)
        : [...roles, role]

      return {
        ...current,
        [userId]: nextRoles,
      }
    })
  }

  async function saveUserRoles(targetUser: UserDto) {
    setIsLoading(true)
    setError('')
    setMessage('')

    try {
      const response = await fetch(`/api/admin/users/${targetUser.id}/roles`, {
        method: 'PUT',
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ roles: draftRoles[targetUser.id] ?? [] }),
      })

      if (!response.ok) {
        const data = (await response.json().catch(() => ({}))) as ApiMessage
        setError(data.message || 'Nao foi possivel atualizar as roles.')
        return
      }

      const updatedUser = (await response.json()) as UserDto
      setUsers((current) =>
        current.map((item) => (item.id === updatedUser.id ? updatedUser : item)),
      )
      setDraftRoles((current) => ({
        ...current,
        [updatedUser.id]: updatedUser.roles,
      }))
      setMessage(`Roles atualizadas para ${updatedUser.userName}.`)
    } catch {
      setError('Nao foi possivel contactar a API de administracao.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <section className="admin-section" aria-labelledby="admin-title">
      <div className="section-heading">
        <div>
          <p className="eyebrow">Administracao</p>
          <h2 id="admin-title">Gestao de utilizadores</h2>
        </div>
        <button type="button" onClick={loadRoleManagement} disabled={isLoading}>
          Atualizar
        </button>
      </div>

      <div className="users-table">
        <div className="users-row users-row-head">
          <span>Utilizador</span>
          <span>Email</span>
          <span>Roles</span>
          <span>Acao</span>
        </div>

        {users.map((managedUser) => (
          <div className="users-row" key={managedUser.id}>
            <span>
              {managedUser.fullName || managedUser.userName}
              {managedUser.id === currentUserId && <small>Conta atual</small>}
            </span>
            <span>{managedUser.email}</span>
            <span className="role-options">
              {availableRoles.map((role) => (
                <label className="role-check" key={role}>
                  <input
                    checked={(draftRoles[managedUser.id] ?? []).includes(role)}
                    type="checkbox"
                    onChange={() => toggleRole(managedUser.id, role)}
                  />
                  {role}
                </label>
              ))}
            </span>
            <span>
              <button type="button" onClick={() => saveUserRoles(managedUser)} disabled={isLoading}>
                Guardar
              </button>
            </span>
          </div>
        ))}
      </div>

      {message && <p className="home-feedback">{message}</p>}
      {error && <p className="feedback error">{error}</p>}
    </section>
  )
}

export default App
